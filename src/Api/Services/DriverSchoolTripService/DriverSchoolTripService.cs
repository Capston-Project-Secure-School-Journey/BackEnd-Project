using Api.Common.Enums;
using Api.Common.Exceptions;
using Api.Domain;
using Api.Domain.Models;
using Api.Extensions;
using Api.Services.ShuttleScheduleManagementService;
using Api.TransferDTOs.Responses;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace Api.Services.DriverSchoolTripService;

public class DriverSchoolTripService(
    Context context,
    IMapper mapper,
    IShuttleScheduleManagementService shuttleScheduleManagementService
)
    : IDriverSchoolTripService
{
    private static readonly SemaphoreSlim SkipLock = new(1, 1);

    public async Task<List<ShuttleScheduleResponse>> GetShuttleScheduleByDate(Guid driverId, DateOnly date)
    {
        var projection = Builders<ShuttleSchedule>.Projection
            .Exclude(x => x.BestRoute)
            .Exclude(x => x.Students);

        var shuttleSchedule = (await context.ShuttleScheduleCollection
                .Find(sp => sp.Date == date && driverId == sp.DriverId)
                .SortByDescending(x => x.SessionType)
                .ThenBy(x => x.Type)
                .ThenByDescending(x => x.NumberOfStudents)
                .Project<ShuttleSchedule>(projection)
                .ToListAsync())
            .Select(mapper.Map<ShuttleScheduleResponse>)
            .ToList();

        return shuttleSchedule;
    }

    public async Task StartJourney(Guid shuttleScheduleId)
    {
        var shuttleSchedule = await shuttleScheduleManagementService.GetShuttleSchedule(shuttleScheduleId);

        if (shuttleSchedule.JourneyStatus != JourneyStatus.NotStarted)
            throw new BadRequestException(
                $"Chuyến đi không thể bắt đầu. Vì trạng thái hiện tại là: {shuttleSchedule.JourneyStatus.GetEnumDisplayName()}");

        if (!shuttleSchedule.IsAllNotesRead)
            throw new BadRequestException(
                $"Chuyến đi không thể bắt đầu. Vì vẫn còn ghi chú từ phụ huynh chưa được đọc.");

        var currentTime = DateTimeHelper.GetDateTimeUtc7();

        if (DateOnly.FromDateTime(currentTime) != shuttleSchedule.Date)
        {
            throw new BadRequestException("Không thể bắt đầu chuyến đi vì chưa phải ngày chạy");
        }

        if (currentTime.TimeOfDay >= shuttleSchedule.PickupStartTime &&
            currentTime.TimeOfDay <= shuttleSchedule.PickupEndTime)
        {
            shuttleSchedule.JourneyStatus = JourneyStatus.InProgress;
            shuttleSchedule.StartJourneyTime = currentTime.TimeOfDay;
            await shuttleScheduleManagementService.UpdateShuttleSchedule(shuttleSchedule);
        }
        else
        {
            throw new BadRequestException(
                $"Chỉ có thể bắt đầu chuyến đi trong khoảng thời gian sau: từ {shuttleSchedule.PickupStartTime.ToString()} " +
                $"đến {shuttleSchedule.PickupEndTime}");
        }
    }

    public async Task EndJourney(Guid shuttleScheduleId)
    {
        var shuttleSchedule = await shuttleScheduleManagementService.GetShuttleSchedule(shuttleScheduleId);

        if (shuttleSchedule.JourneyStatus != JourneyStatus.InProgress)
            throw new BadRequestException(
                $"Chuyến đi không thể kết thúc. Vì trạng thái hiện tại là: {shuttleSchedule.JourneyStatus.GetEnumDisplayName()}");

        var students = shuttleSchedule.Students;
        var studentNotPickedUp = students
            .FirstOrDefault(st => !st.SkipPickup && (!st.IsPickedUp || !st.IsDroppedOff));

        if (studentNotPickedUp != null)
            throw new BadRequestException($"Vẫn còn học sinh {studentNotPickedUp.FullName} chưa được đón hoặc trả.\n" +
                                          $"Nếu học sinh quên quét qr thì hãy bấm bỏ quả học sinh và kèm lí do quên quét qr." +
                                          $"Nếu không phải hãy thực hiện đưa đón đầy đủ tất cả học sinh.");

        var currentTime = DateTimeHelper.GetDateTimeUtc7();

        shuttleSchedule.JourneyStatus = JourneyStatus.Completed;
        shuttleSchedule.EndJourneyTime = currentTime.TimeOfDay;
        await shuttleScheduleManagementService.UpdateShuttleSchedule(shuttleSchedule);

        var activeDriver = await context.ActiveDrivers.FirstOrDefaultAsync(ad =>
            ad.DriverId == shuttleSchedule.DriverId && ad.SchoolId == shuttleSchedule.SchoolId);

        activeDriver!.TotalDistanceKm += shuttleSchedule.TotalDistanceKm;
        context.ActiveDrivers.Update(activeDriver);
        await context.SaveChangesAsync();
    }

    public async Task CancelJourney(Guid shuttleScheduleId, string cancelReason)
    {
        var shuttleSchedule = await shuttleScheduleManagementService.GetShuttleSchedule(shuttleScheduleId);

        if (shuttleSchedule.JourneyStatus is JourneyStatus.Completed or JourneyStatus.Cancelled)
            throw new BadRequestException(
                $"Chuyến đi không thể hủy. Vì trạng thái hiện tại là: {shuttleSchedule.JourneyStatus.GetEnumDisplayName()}");

        shuttleSchedule.JourneyStatus = JourneyStatus.Cancelled;
        shuttleSchedule.CancelReason = cancelReason;
        await shuttleScheduleManagementService.UpdateShuttleSchedule(shuttleSchedule);
    }

    public async Task SkipStudentByDriver(Guid shuttleScheduleId, Guid studentId, string cancelReason)
    {
        var shuttleSchedule = await shuttleScheduleManagementService.GetShuttleSchedule(shuttleScheduleId);
        if (shuttleSchedule.JourneyStatus != JourneyStatus.InProgress)
            throw new BadRequestException("Chuyến đi chưa bắt đầu.");

        await SkipStudent(shuttleScheduleId, studentId, cancelReason);
    }

    public async Task SkipStudent(Guid shuttleScheduleId, Guid studentId, string cancelReason)
    {
        await SkipLock.WaitAsync();

        try
        {
            var shuttleSchedule = await shuttleScheduleManagementService.GetShuttleSchedule(shuttleScheduleId);
            var studentOnBus = shuttleSchedule.Students.FirstOrDefault(st => st.StudentId == studentId);

            if (studentOnBus == null)
                throw new BadRequestException("Học sinh không tồn tại trên chuyến này.");

            studentOnBus.SkipPickup = true;
            studentOnBus.IsSkipUpReason = cancelReason;
            shuttleSchedule.NumberOfStudents -= 1;

            await shuttleScheduleManagementService.UpdateShuttleSchedule(shuttleSchedule);
            await shuttleScheduleManagementService.UpdateStudentOnShuttleSchedule(shuttleSchedule.Id, studentOnBus);
        }
        finally
        {
            SkipLock.Release();
        }
    }

    public async Task UndoSkipStudent(Guid shuttleScheduleId, Guid studentId)
    {
        await SkipLock.WaitAsync();

        try
        {
            var shuttleSchedule = await shuttleScheduleManagementService.GetShuttleSchedule(shuttleScheduleId);
            var studentOnBus = shuttleSchedule.Students.FirstOrDefault(st => st.StudentId == studentId);

            if (studentOnBus == null)
                throw new BadRequestException("Học sinh không tồn tại trên chuyến này.");

            studentOnBus.SkipPickup = false;
            studentOnBus.IsSkipUpReason = string.Empty;
            shuttleSchedule.NumberOfStudents += 1;

            await shuttleScheduleManagementService.UpdateShuttleSchedule(shuttleSchedule);
            await shuttleScheduleManagementService.UpdateStudentOnShuttleSchedule(shuttleSchedule.Id, studentOnBus);
        }
        finally
        {
            SkipLock.Release();
        }
    }

    public async Task<bool> HasInProgressShuttle(Guid driverId)
    {
        var currentTime = DateTimeHelper.GetDateTimeUtc7();
        return await context.ShuttleScheduleCollection
            .Find(ss => ss.Date == DateOnly.FromDateTime(currentTime)
                        && ss.JourneyStatus == JourneyStatus.InProgress
                        && ss.DriverId == driverId)
            .AnyAsync();
    }

    public async Task<ShuttleSchedule> GetCurrentShuttleScheduleByDriver(Guid driverId)
    {
        var currentTime = DateTimeHelper.GetDateTimeUtc7();
        var shuttleSchedule = await context.ShuttleScheduleCollection
            .Find(ss => ss.Date == DateOnly.FromDateTime(currentTime)
                        && ss.JourneyStatus == JourneyStatus.InProgress
                        && ss.DriverId == driverId)
            .FirstOrDefaultAsync();

        if (shuttleSchedule == null)
        {
            throw new BadRequestException("Bạn không có chuyến đi nào hiện tại.");
        }

        return shuttleSchedule;
    }

    public async Task<bool> HasUpcomingShuttle(Guid driverId)
    {
        var currentTime = DateTimeHelper.GetDateTimeUtc7().TimeOfDay;
        return await context.ShuttleScheduleCollection
            .Find(ss => ss.Date == DateOnly.FromDateTime(DateTimeHelper.GetDateTimeUtc7())
                        && ss.PickupStartTime <= currentTime
                        && ss.PickupEndTime >= currentTime
                        && ss.JourneyStatus == JourneyStatus.NotStarted
                        && ss.DriverId == driverId)
            .AnyAsync();
    }

    public async Task<ShuttleSchedule> GetUpcomingShuttleSchedule(Guid driverId)
    {
        var currentTime = DateTimeHelper.GetDateTimeUtc7().TimeOfDay;
        var shuttleSchedule = await context.ShuttleScheduleCollection
            .Find(ss => ss.Date == DateOnly.FromDateTime(DateTimeHelper.GetDateTimeUtc7())
                        && ss.PickupStartTime <= currentTime
                        && ss.PickupEndTime >= currentTime
                        && ss.JourneyStatus == JourneyStatus.NotStarted
                        && ss.DriverId == driverId)
            .FirstOrDefaultAsync();

        return shuttleSchedule;
    }

    public async Task IsOwnerOfShuttleSchedule(Guid shuttleScheduleId, Guid driverId)
    {
        var exist = await context.ShuttleScheduleCollection
            .Find(ss => ss.DriverId == driverId && ss.Id == shuttleScheduleId)
            .AnyAsync();

        if (!exist)
            throw new NotFoundException("Không tồn tại lịch đưa đón.");
    }

    public async Task UpdateCurrentAddress(Guid shuttleScheduleId, Guid driveId, double lat, double lng)
    {
        var filter = Builders<ShuttleSchedule>.Filter.And(
            Builders<ShuttleSchedule>.Filter.Eq(t => t.Id, shuttleScheduleId),
            Builders<ShuttleSchedule>.Filter.Eq(t => t.DriverId, driveId)
        );

        var update = Builders<ShuttleSchedule>.Update
            .Set(s => s.CurrentLat, lat)
            .Set(s => s.CurrentLng, lng);

        await context.ShuttleScheduleCollection.UpdateOneAsync(filter, update);
    }
}