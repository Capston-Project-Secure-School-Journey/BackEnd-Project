using Api.TransferDTOs.Responses;

namespace Api.Services.ParentSchoolTripService;

public class ParentSchoolTripHandler(IParentSchoolTripService parentSchoolTripService) : IParentSchoolTripHandler
{
    public async Task<List<ParentShuttleScheduleResponse>> GetShuttleSchedulesByStudent(Guid parentId, Guid studentId,
        DateOnly date)
    {
        await parentSchoolTripService.IsManageByStudent(parentId, studentId);
        return await parentSchoolTripService.GetShuttleSchedulesByStudent(studentId, date);
    }

    public async Task<bool> HasInProgressShuttle(Guid parentId)
    {
        return await parentSchoolTripService.HasInProgressShuttle(parentId);
    }

    public async Task<List<ParentShuttleScheduleResponse>> GetCurrentShuttleSchedule(Guid parentId)
    {
        return await parentSchoolTripService.GetCurrentShuttleSchedule(parentId);
    }

    public async Task<bool> HasUpcomingShuttle(Guid parentId)
    {
        return await parentSchoolTripService.HasUpcomingShuttle(parentId);
    }

    public async Task<List<ParentShuttleScheduleResponse>> GetUpcomingShuttleSchedule(Guid parentId)
    {
        return await parentSchoolTripService.GetUpcomingShuttleSchedule(parentId);
    }
}