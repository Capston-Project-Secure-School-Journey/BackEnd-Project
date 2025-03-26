using Api.Domain.Models;
using Api.DTOs.ScheduleManagement;
using Api.TransferDTOs.Responses;

namespace Api.Services.ScheduleManagementService;

public interface IScheduleManagementService
{
    Task<IEnumerable<ClassSchedule>> CreateSchedule(Guid schoolId, CreateScheduleDto dto);
    Task<ClassSchedule> UpdateSchedule(Guid schoolId, UpdateScheduleDto dto);
    Task DeleteSchedule(Guid schoolId, Guid id);
    Task DeleteSchedule(Guid schoolId, List<Guid> ids);
    Task<IEnumerable<ClassSchedule>> GetScheduleByMonth(Guid schoolId, DateOnly date);
    Task<IEnumerable<ClassSchedule>> GetScheduleByWeek(Guid schoolId, DateTime date);
    Task<IEnumerable<ClassSchedule>> GetScheduleByDate(Guid schoolId, DateTime date);
    Task<ClassSchedulePaginationResponse> GetScheduleView(Guid schoolId, DateOnly date);
    Task<IEnumerable<ClassSchedule>> CloneMonthSchedule(Guid schoolId, DateOnly date);
    Task<IEnumerable<ClassSchedule>> CloneWeekSchedule(Guid schoolId, DateOnly date);
    Task<IEnumerable<ClassSchedule>> CloneDaySchedule(Guid schoolId, DateOnly date);
}