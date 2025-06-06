using Api.Common.Enums;
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
    Task<IEnumerable<ClassSchedule>> GetScheduleByWeek(Guid schoolId, DateTime date);

    Task<IQueryable<ClassSchedule>> GetScheduleByDateQueryable(Guid schoolId,
        DateOnly date,
        SessionType? sessionType,
        Guid? classId,
        string? className,
        Grade? grade);

    Task<ClassSchedulePaginationResponse> GetScheduleView(Guid schoolId, DateOnly date);
    Task<IEnumerable<ClassSchedule>> CloneMonthSchedule(Guid schoolId, DateOnly date);
    Task<IEnumerable<ClassSchedule>> CloneWeekSchedule(Guid schoolId, DateOnly date);
    Task<IEnumerable<ClassSchedule>> CloneDaySchedule(Guid schoolId, DateOnly date);
}