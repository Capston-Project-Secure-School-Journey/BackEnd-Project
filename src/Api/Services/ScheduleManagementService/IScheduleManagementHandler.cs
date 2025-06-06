using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;

namespace Api.Services.ScheduleManagementService;

public interface IScheduleManagementHandler
{
    Task<IEnumerable<ClassScheduleResponse>> CreateSchedule(Guid schoolId, CreateScheduleRequest request);
    Task<ClassScheduleResponse> UpdateSchedule(Guid schoolId, UpdateScheduleRequest request);
    Task<ClassSchedulePaginationResponse> GetScheduleView(Guid schoolId, DateOnly date);
    Task<Pagination<ClassScheduleResponse>> GetScheduleByDate(Guid schoolId, GetScheduleByDateRequest request);
    Task DeleteSchedule(Guid schoolId, Guid id);
    Task DeleteSchedule(Guid schoolId, List<Guid> ids);
}