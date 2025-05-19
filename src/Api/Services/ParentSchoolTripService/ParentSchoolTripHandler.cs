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
}