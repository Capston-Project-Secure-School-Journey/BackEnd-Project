using Api.Common.Enums;
using Api.Common.Exceptions;
using Api.Common.Utilities;
using Api.Domain;
using Api.Domain.Models;
using Api.Extensions;
using Api.Services.UserService;
using Api.TransferDTOs.Responses;
using AutoMapper;
using MongoDB.Driver;

namespace Api.Services.ParentSchoolTripService;

public class ParentSchoolTripService(
    Context context,
    IMapper mapper,
    IUserService userService) : IParentSchoolTripService
{
    public async Task<List<ParentShuttleScheduleResponse>> GetShuttleSchedulesByStudent(Guid studentId, DateOnly date)
    {
        var projection = Builders<ShuttleSchedule>.Projection
            .Exclude(x => x.BestRoute);

        var shuttleSchedules = await context.ShuttleScheduleCollection
            .Find(ss => ss.Date == date
                        && ss.Students.Any(st => st.StudentId == studentId))
            .SortByDescending(x => x.SessionType)
            .ThenBy(x => x.Type)
            .Project<ShuttleSchedule>(projection)
            .ToListAsync();
        return shuttleSchedules
            .Select(x => ConvertShuttleScheduleResponse2ParentResponse(x, studentId))
            .ToList();
    }

    public async Task<bool> HasInProgressShuttle(Guid parentId)
    {
        return await context.ShuttleScheduleCollection.Find(ss =>
            ss.Date == DateTimeHelper.GetDateTimeOnlyUtc7() &&
            ss.JourneyStatus == JourneyStatus.InProgress &&
            ss.Students.Any(st => st.Parents.Any(pr => pr.ParentId == parentId) && !st.SkipPickup)
        ).AnyAsync();
    }

    public async Task<List<ParentShuttleScheduleResponse>> GetCurrentShuttleSchedule(Guid parentId)
    {
        var projection = Builders<ShuttleSchedule>.Projection
            .Exclude(x => x.BestRoute);

        var trips = (await context.ShuttleScheduleCollection.Find(ss =>
                ss.Date == DateTimeHelper.GetDateTimeOnlyUtc7() &&
                ss.JourneyStatus == JourneyStatus.InProgress &&
                ss.Students.Any(st => st.Parents.Any(pr => pr.ParentId == parentId) && !st.SkipPickup)
            )
            .Project<ShuttleSchedule>(projection)
            .ToListAsync());

        var response = new List<ParentShuttleScheduleResponse>();
        foreach (var trip in trips)
        {
            foreach (var student in trip.Students
                         .Where(x => x.Parents.Any(y => y.ParentId == parentId)
                                     && !x.SkipPickup)
                    )
            {
                response.Add(ConvertShuttleScheduleResponse2ParentResponse(trip, student.StudentId));
            }
        }

        return response;
    }

    public async Task<bool> HasUpcomingShuttle(Guid parentId)
    {
        var currentTime = DateTimeHelper.GetDateTimeUtc7().TimeOfDay;
        return await context.ShuttleScheduleCollection.Find(ss =>
            ss.Date == DateTimeHelper.GetDateTimeOnlyUtc7() &&
            ss.PickupStartTime <= currentTime &&
            ss.PickupEndTime >= currentTime &&
            ss.JourneyStatus == JourneyStatus.NotStarted &&
            ss.Students.Any(st => st.Parents.Any(pr => pr.ParentId == parentId) && !st.SkipPickup)
        ).AnyAsync();
    }

    public async Task<List<ParentShuttleScheduleResponse>> GetUpcomingShuttleSchedule(Guid parentId)
    {
        var projection = Builders<ShuttleSchedule>.Projection
            .Exclude(x => x.BestRoute);

        var currentTime = DateTimeHelper.GetDateTimeUtc7().TimeOfDay;
        var trips = await context.ShuttleScheduleCollection.Find(ss =>
                ss.Date == DateTimeHelper.GetDateTimeOnlyUtc7() &&
                ss.PickupStartTime <= currentTime &&
                ss.PickupEndTime >= currentTime &&
                ss.JourneyStatus == JourneyStatus.NotStarted &&
                ss.Students.Any(st => st.Parents.Any(pr => pr.ParentId == parentId) && !st.SkipPickup)
            ).Project<ShuttleSchedule>(projection)
            .ToListAsync();

        var response = new List<ParentShuttleScheduleResponse>();
        foreach (var trip in trips)
        {
            foreach (var student in trip.Students
                         .Where(x => x.Parents.Any(y => y.ParentId == parentId)
                                     && !x.SkipPickup)
                    )
            {
                response.Add(ConvertShuttleScheduleResponse2ParentResponse(trip, student.StudentId));
            }
        }

        return response;
    }

    public async Task IsManageByStudent(Guid parentId, Guid studentId)
    {
        var parent = await userService.GetUser(parentId, UserType.Parent) as Parent;

        if (parent == null || parent.RelationshipWithStudents.All(x => x.StudentId != studentId))
            throw new BadRequestException(ErrorMessages.AccessDenied);
    }

    private ParentShuttleScheduleResponse ConvertShuttleScheduleResponse2ParentResponse(
        ShuttleSchedule shuttleSchedule,
        Guid studentId)
    {
        var shuttleScheduleResponse = mapper.Map<ParentShuttleScheduleResponse>(shuttleSchedule);
        var student = shuttleSchedule.Students.First(x => x.StudentId == studentId);
        shuttleScheduleResponse.IsPickedUp = student.IsPickedUp;
        shuttleScheduleResponse.IsDroppedOff = student.IsDroppedOff;
        shuttleScheduleResponse.PickedUpTime = student.PickedUpTime?.DateTime;
        shuttleScheduleResponse.DroppedOffTime = student.DroppedOffTime?.DateTime;
        shuttleScheduleResponse.SkipPickup = student.SkipPickup;
        shuttleScheduleResponse.IsSkipUpReason = student.IsSkipUpReason;
        shuttleScheduleResponse.ClassId = student.ClassId;
        shuttleScheduleResponse.FullName = student.FullName;
        shuttleScheduleResponse.StudentId = student.StudentId;
        shuttleScheduleResponse.ClassName = student.ClassName;
        return shuttleScheduleResponse;
    }
}