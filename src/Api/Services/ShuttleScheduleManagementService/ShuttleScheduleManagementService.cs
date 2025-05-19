using Api.Common.Enums;
using Api.Common.Exceptions;
using Api.Common.Utilities;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.ShuttleScheduleService;
using Api.Extensions;
using Api.Scheduling;
using Api.Services.SchoolManagement;
using Api.Services.UploadFileService;
using Api.Services.UserService;
using Api.TransferDTOs.Responses;
using AutoMapper;
using MongoDB.Driver;

namespace Api.Services.ShuttleScheduleManagementService;

public class ShuttleScheduleManagementService(
    Context context,
    IUserService userService,
    IFileUploadService fileUploadService,
    IMapper mapper,
    ISchoolManagement schoolManagement,
    GoogleMapsService googleMapsService) : IShuttleScheduleManagementService
{
    public async Task UpdateShuttleSchedule(ShuttleSchedule shuttleSchedule)
    {
        var filter = Builders<ShuttleSchedule>.Filter.Eq(s => s.Id, shuttleSchedule.Id);
        var update = Builders<ShuttleSchedule>.Update
            .Set(s => s.JourneyStatus, shuttleSchedule.JourneyStatus)
            .Set(s => s.EndTime, shuttleSchedule.EndTime)
            .Set(s => s.CancelReason, shuttleSchedule.CancelReason)
            .Set(s => s.NumberOfPickedUpStudents, shuttleSchedule.NumberOfPickedUpStudents)
            .Set(s => s.NumberOfDroppedOffStudents, shuttleSchedule.NumberOfDroppedOffStudents);

        await context.ShuttleScheduleCollection.UpdateOneAsync(filter, update);
    }

    public async Task UpdateStudentOnShuttleSchedule(Guid shuttleScheduleId, StudentOnBus studentOnBus)
    {
        var filter = Builders<ShuttleSchedule>.Filter.And(
            Builders<ShuttleSchedule>.Filter.Eq(t => t.Id, shuttleScheduleId),
            Builders<ShuttleSchedule>.Filter.ElemMatch(t => t.Students, s => s.StudentId == studentOnBus.StudentId)
        );

        var update = Builders<ShuttleSchedule>.Update
            .Set("Students.$.IsPickedUp", studentOnBus.IsPickedUp)
            .Set("Students.$.IsDroppedOff", studentOnBus.IsDroppedOff)
            .Set("Students.$.PickedUpTime", studentOnBus.PickedUpTime)
            .Set("Students.$.DroppedOffTime", studentOnBus.DroppedOffTime)
            .Set("Students.$.IsSkipUpReason", studentOnBus.IsSkipUpReason)
            .Set("Students.$.SkipPickup", studentOnBus.SkipPickup);
        
        await context.ShuttleScheduleCollection.UpdateOneAsync(filter, update);
    }

    public async Task<List<ShuttleSchedule>> AddShuttleSchedule(List<CreateShuttleScheduleDto> requests)
    {
        var shuttleSchedules = new List<ShuttleSchedule>();
        var tasks = new List<Task>();
        foreach (var request in requests)
        {
            tasks.Add(DeleteShuttleSchedule(request));
            var shuttleSchedule = await CreateShuttleScheduleFromDto(request);
            shuttleSchedules.Add(shuttleSchedule);
        }

        await Task.WhenAll(tasks);
        await context.ShuttleScheduleCollection.InsertManyAsync(shuttleSchedules);

        return shuttleSchedules;
    }

    public async Task<ShuttleScheduleView> GetShuttleScheduleView(DateOnly date, Guid schoolId)
    {
        var monthRange = DateTimeHelper.GetMonthRange(date);
        var shuttleSchedules = (await context.ShuttleScheduleCollection
            .Aggregate()
            .Group(
                key => new
                {
                    key.Date,
                    key.SchoolId,
                    key.SessionType,
                    key.Type
                },
                g => new ShuttleScheduleGroupResult
                {
                    Id = new GroupKey
                    {
                        Date = g.Key.Date,
                        SchoolId = g.Key.SchoolId,
                        SessionType = g.Key.SessionType,
                        Type = g.Key.Type
                    },
                    TotalStudents = g.Sum(x => x.NumberOfStudents),
                    TotalTrips = g.Count(),
                }
            )
            .Sort(Builders<ShuttleScheduleGroupResult>.Sort
                .Descending(x => x.Id.Date)
                .Descending(x => x.Id.SessionType)
                .Ascending(x => x.Id.Type)
            )
            .ToListAsync())
            .Where(g => g.Id.SchoolId == schoolId 
                        && g.Id.Date >= monthRange.StartOfMonth 
                        && g.Id.Date <= monthRange.EndOfMonth);

        var response = new ShuttleScheduleView();
        foreach (var sp in shuttleSchedules)
        {
            if (response.Data.TryGetValue(sp.Id.Date, out _))
            {
                response.Data[sp.Id.Date].Add(new()
                {
                    Date = sp.Id.Date,
                    SchoolId = sp.Id.SchoolId,
                    Type = sp.Id.Type,
                    SessionType = sp.Id.SessionType,
                    NumberOfStudents = sp.TotalStudents,
                    NumberOfTrips = sp.TotalTrips
                });
            }
            else
            {
                response.Data.Add(sp.Id.Date, [
                    new()
                    {
                        Date = sp.Id.Date,
                        SchoolId = sp.Id.SchoolId,
                        Type = sp.Id.Type,
                        SessionType = sp.Id.SessionType,
                        NumberOfStudents = sp.TotalStudents,
                        NumberOfTrips = sp.TotalTrips
                    }
                ]);
            }
        }

        return response;
    }

    public async Task<List<ShuttleScheduleResponse>> GetShuttleScheduleByDate(DateOnly date, Guid schoolId)
    {
        var projection = Builders<ShuttleSchedule>.Projection
            .Exclude(x => x.BestRoute)
            .Exclude(x => x.Students);

        var shuttleSchedule = (await context.ShuttleScheduleCollection
                .Find(sp => sp.Date == date && sp.SchoolId == schoolId)
                .SortByDescending(x => x.SessionType)
                .ThenBy(x => x.Type)
                .ThenByDescending(x => x.NumberOfStudents)
                .Project<ShuttleSchedule>(projection)
                .ToListAsync())
            .Select(mapper.Map<ShuttleScheduleResponse>)
            .ToList();

        return shuttleSchedule;
    }

    public async Task<ShuttleSchedule> GetShuttleSchedule(Guid shuttleScheduleId)
    {
        var shuttleSchedule = await context.ShuttleScheduleCollection
            .Find(sp => sp.Id == shuttleScheduleId)
            .FirstOrDefaultAsync<ShuttleSchedule>();

        if (shuttleSchedule == null)
            throw new NotFoundException("Shuttle schedule not found");

        return shuttleSchedule;
    }

    public async Task IsOwnerOfShuttleSchedule(Guid shuttleScheduleId, Guid schoolId)
    {
        var exist = await context.ShuttleScheduleCollection
            .Find(sp => sp.Id == shuttleScheduleId && sp.SchoolId == schoolId)
            .AnyAsync<ShuttleSchedule>();

        if (!exist)
            throw new NotFoundException("Shuttle schedule not found");
    }

    private async Task<ShuttleSchedule> CreateShuttleScheduleFromDto(CreateShuttleScheduleDto shuttleScheduleDto)
    {
        var driver = await userService.GetUser(shuttleScheduleDto.DriverId, UserType.Driver) as Driver;
        var school = await schoolManagement.GetSchool(shuttleScheduleDto.SchoolId);

        if (driver == null)
            throw new NotFoundException(ErrorMessages.UserNotFound);

        var shuttleSchedule = new ShuttleSchedule()
        {
            DriverId = shuttleScheduleDto.DriverId,
            SchoolId = shuttleScheduleDto.SchoolId,
            SessionType = shuttleScheduleDto.SessionType,
            SchoolName = school.SchoolName,
            Date = shuttleScheduleDto.Date,
            DriverName = driver.FirstName + " " + driver.LastName,
            DriverAvatar = driver.AvatarKey == null
                ? ""
                : await fileUploadService.GeneratePreSignedDownloadUrlAsync(driver.AvatarKey.Value, 99999),
            VehicleType = driver.VehicleType,
            DriverGender = driver.Gender,
            LicenseNumber = driver.LicenseNumber,
            IsAllNotesRead = true,
            Type = shuttleScheduleDto.Type,
            StartTime = GetStartTime(shuttleScheduleDto.Type, shuttleScheduleDto.SessionType, school),
            EndTime = null,
            NumberOfStudents = shuttleScheduleDto.Students.Count,
            NumberOfPickedUpStudents = 0,
            NumberOfDroppedOffStudents = 0,
            JourneyStatus = JourneyStatus.NotStarted,
            Students = new()
        };


        foreach (var student in shuttleScheduleDto.Students)
        {
            var studentOnBus = new StudentOnBus()
            {
                StudentId = student.Id,
                Parents = student.ManagedBy,
                PickupAddress = student.PickUpLocation,
                PickupLat = student.PickUpLat,
                PickupLng = student.PickUpLng,
                Gender = student.Gender,
                AvatarUrl = student.AvatarKey == null
                    ? ""
                    : await fileUploadService.GeneratePreSignedDownloadUrlAsync(student.AvatarKey.Value, 99999),
                ClassName = student.Class.ClassName,
                ClassId = student.ClassId,
                FullName = student.FullName,
                IsPickedUp = false,
                PickedUpTime = null,
                IsDroppedOff = false,
                DroppedOffTime = null,
                SkipPickup = false
            };
            shuttleSchedule.Students.Add(studentOnBus);
        }

        await GetBestRoute(shuttleSchedule, school);
        return shuttleSchedule;
    }

    private async Task DeleteShuttleSchedule(CreateShuttleScheduleDto shuttleScheduleDto)
    {
        var shuttleScheduleCollection = context.MongoDatabase.GetCollection<ShuttleSchedule>("pickup_schedules");
        await shuttleScheduleCollection.DeleteOneAsync(pk =>
            pk.SchoolId == shuttleScheduleDto.SchoolId
            && pk.Date == shuttleScheduleDto.Date
            && pk.SessionType == shuttleScheduleDto.SessionType);
    }

    private static TimeSpan GetStartTime(ShuttleScheduleType shuttleScheduleType, SessionType sessionType, School school)
    {
        switch (shuttleScheduleType)
        {
            case ShuttleScheduleType.PickUp:
                if (sessionType == SessionType.Morning)
                    return school.MorningStartTime - new TimeSpan(1, 0, 0);
                return school.AfternoonStartTime - new TimeSpan(1, 0, 0);
            case ShuttleScheduleType.DropOff:
                if (sessionType == SessionType.Afternoon)
                    return school.AfternoonEndTime;
                return school.MorningEndTime;
            default:
                throw new InvalidDataException("Invalid pickup schedule type");
        }
    }

    private async Task GetBestRoute(ShuttleSchedule shuttleSchedule, School school)
    {
        string origin;
        string destination;
        var firstStudent = shuttleSchedule
            .Students
            .OrderByDescending(x =>
                IStudentGroupingAlgorithm.Haversine(x.PickupLat, x.PickupLng, school.AddressLat,
                    school.AddressLng))
            .FirstOrDefault();
        if (firstStudent == null)
            throw new InvalidDataException("No first student found");

        var waypoints = shuttleSchedule
            .Students
            .Where(x => x.StudentId != firstStudent.StudentId)
            .Select(x => x.PickupLat + "," + x.PickupLng)
            .ToList();

        if (shuttleSchedule.Type == ShuttleScheduleType.PickUp)
        {
            origin = firstStudent.PickupLat + "," + firstStudent.PickupLng;
            destination = school.AddressLat + "," + school.AddressLng;
        }
        else
        {
            origin = school.AddressLat + "," + school.AddressLng;
            destination = firstStudent.PickupLat + "," + firstStudent.PickupLng;
        }

        shuttleSchedule.BestRoute = (await googleMapsService.GetOptimizedRouteAsync(origin, destination, waypoints)).Item2;
    }
}