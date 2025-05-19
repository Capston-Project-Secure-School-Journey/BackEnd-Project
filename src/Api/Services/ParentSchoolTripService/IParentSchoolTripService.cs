using Api.TransferDTOs.Responses;

namespace Api.Services.ParentSchoolTripService;

public interface IParentSchoolTripService
{
    Task<List<ParentShuttleScheduleResponse>> GetShuttleSchedulesByStudent(Guid studentId, DateOnly date);
    Task IsManageByStudent(Guid parentId, Guid studentId);
}