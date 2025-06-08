using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.ScheduleManagement;
using Api.Extensions;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

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

    public async Task<Pagination<ClassScheduleResponse>> GetScheduleByDate(Guid schoolId,
        GetScheduleByDateRequest request)
    {
        var query = await scheduleManagementService.GetScheduleByDateQueryable(schoolId,
            request.Date,
            request.SessionType,
            request.ClassId,
            request.ClassName,
            request.Grade);

        var total = await query.CountAsync();

        var data = query
            .SortByProperty(request.SortBy, request.Direction)
            .Pagination(request.Page, request.Limit)
            .AsEnumerable()
            .Select(MapToClassScheduleResponse);

        var response = new Pagination<ClassScheduleResponse>(data, request.Limit, request.Page, total);

        return response;
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
        finally
        {
            await trans.DisposeAsync();
        }
    }

    public async Task CloneWeekSchedule(Guid schoolId, DateOnly weekSource, DateOnly weekDestination)
    {
        var trans = await context.Database.BeginTransactionAsync();
        try
        {
            await scheduleManagementService.CloneWeekSchedule(schoolId, weekSource, weekDestination);
            await trans.CommitAsync();
        }
        finally
        {
            await trans.DisposeAsync();
        }
    }

    public async Task CloneDaySchedule(Guid schoolId, DateOnly dateSource, DateOnly dateDestination)
    {
        var trans = await context.Database.BeginTransactionAsync();
        try
        {
            await scheduleManagementService.CloneDaySchedule(schoolId, dateSource, dateDestination);
            await trans.CommitAsync();
        }
        finally
        {
            await trans.DisposeAsync();
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