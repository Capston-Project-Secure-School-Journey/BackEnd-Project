using Api.TransferDTOs.Responses;

namespace Api.Services.ParentSchoolTripService;

public interface IParentSchoolTripHandler
{
    Task<List<ParentShuttleScheduleResponse>>
        GetShuttleSchedulesByStudent(Guid parentId, Guid studentId, DateOnly date);
}