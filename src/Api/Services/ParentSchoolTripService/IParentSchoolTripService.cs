using Api.TransferDTOs.Responses;

namespace Api.Services.ParentSchoolTripService;

public interface IParentSchoolTripService
{
    Task<List<ParentShuttleScheduleResponse>> GetShuttleSchedulesByStudent(Guid studentId, DateOnly date);
    Task<bool> HasInProgressShuttle(Guid parentId);
    Task<List<ParentShuttleScheduleResponse>> GetCurrentShuttleSchedule(Guid parentId);
    Task<bool> HasUpcomingShuttle(Guid parentId);
    Task<List<ParentShuttleScheduleResponse>> GetUpcomingShuttleSchedule(Guid parentId);
    Task IsManageByStudent(Guid parentId, Guid studentId);
}