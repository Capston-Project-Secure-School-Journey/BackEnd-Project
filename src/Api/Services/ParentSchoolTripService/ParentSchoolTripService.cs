using Api.Common.Enums;
using Api.Common.Exceptions;
using Api.Common.Utilities;
using Api.Domain;
using Api.Domain.Models;
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

        var response = new List<ParentShuttleScheduleResponse>();
        foreach (var shuttleSchedule in shuttleSchedules)
        {
            var shuttleScheduleResponse = mapper.Map<ParentShuttleScheduleResponse>(shuttleSchedule);
            var student = shuttleSchedule.Students.First(x => x.StudentId == studentId);
            shuttleScheduleResponse.IsPickedUp = student.IsPickedUp;
            shuttleScheduleResponse.IsDroppedOff = student.IsDroppedOff;
            shuttleScheduleResponse.PickedUpTime = student.PickedUpTime;
            shuttleScheduleResponse.DroppedOffTime = student.DroppedOffTime;
            shuttleScheduleResponse.SkipPickup = student.SkipPickup;
            shuttleScheduleResponse.IsSkipUpReason = student.IsSkipUpReason;
            response.Add(shuttleScheduleResponse);
        }

        return response;
    }

    public async Task IsManageByStudent(Guid parentId, Guid studentId)
    {
        var parent = await userService.GetUser(parentId, UserType.Parent) as Parent;

        if (parent == null || parent.RelationshipWithStudents.All(x => x.StudentId != studentId))
            throw new BadRequestException(ErrorMessages.AccessDenied);
    }
}