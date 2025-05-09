using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.ScheduleManagement;
using Api.Extensions;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;
using AutoMapper;

namespace Api.Services.ScheduleManagementService;

public class ScheduleManagementHandler(
    IScheduleManagementService scheduleManagementService,
    IMapper mapper,
    Context context)
    : IScheduleManagementHandler
{
    public async Task<IEnumerable<ClassScheduleResponse>> CreateSchedule(Guid schoolId, CreateScheduleRequest request)
    {
        var dto = mapper.Map<CreateScheduleDto>(request);
        var trans = await context.Database.BeginTransactionAsync();
        try
        {
            var classSchedules = await scheduleManagementService.CreateSchedule(schoolId, dto);
            await trans.CommitAsync();
            return classSchedules
                .Select(mapper.Map<ClassScheduleResponse>)
                .ToList();
        }
        catch (Exception)
        {
            await trans.DisposeAsync();
            throw;
        }
    }

    public async Task<ClassScheduleResponse> UpdateSchedule(Guid schoolId, UpdateScheduleRequest request)
    {
        var dto = mapper.Map<UpdateScheduleDto>(request);
        var classSchedule = await scheduleManagementService.UpdateSchedule(schoolId, dto);
        return mapper.Map<ClassScheduleResponse>(classSchedule);
    }

    public async Task<ClassSchedulePaginationResponse> GetScheduleView(Guid schoolId, DateOnly date)
    {
        return await scheduleManagementService.GetScheduleView(schoolId, date);
    }

    public async Task<List<ClassScheduleResponse>> GetScheduleByDate(Guid schoolId, DateOnly date)
    {
        var classSchedules = await scheduleManagementService.GetScheduleByDate(schoolId, date);

        return classSchedules.Select(MapToClassScheduleResponse)
            .ToList();
    }

    public async Task DeleteSchedule(Guid schoolId, Guid id)
    {
        await scheduleManagementService.DeleteSchedule(schoolId, id);
    }

    public async Task DeleteSchedule(Guid schoolId, List<Guid> ids)
    {
        var trans = await context.Database.BeginTransactionAsync();
        try
        {
            await scheduleManagementService.DeleteSchedule(schoolId, ids);
            await trans.CommitAsync();
        }
        catch (Exception)
        {
            await trans.RollbackAsync();
            throw;
        }
    }

    private ClassScheduleResponse MapToClassScheduleResponse(ClassSchedule schedule)
    {
        var response = mapper.Map<ClassScheduleResponse>(schedule);
        response.ClassName = schedule.Class.ClassName;
        response.Grade = schedule.Class.Grade;
        response.GradeName = schedule.Class.Grade.GetEnumDisplayName();

        return response;
    }
}