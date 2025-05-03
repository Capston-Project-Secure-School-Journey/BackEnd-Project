using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.ScheduleManagement;
using Api.Extensions;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;
using AutoMapper;

namespace Api.Services.ScheduleManagementService;

public class ScheduleManagementHandler : IScheduleManagementHandler
{
    private readonly IScheduleManagementService _scheduleManagementService;
    private readonly IMapper _mapper;
    private readonly Context _context;

    public ScheduleManagementHandler(IScheduleManagementService scheduleManagementService,
        IMapper mapper,
        Context context)
    {
        _scheduleManagementService = scheduleManagementService;
        _mapper = mapper;
        _context = context;
    }

    public async Task<IEnumerable<ClassScheduleResponse>> CreateSchedule(Guid schoolId, CreateScheduleRequest request)
    {
        var dto = _mapper.Map<CreateScheduleDto>(request);
        var trans = await _context.Database.BeginTransactionAsync();
        try
        {
            var classSchedules = await _scheduleManagementService.CreateSchedule(schoolId, dto);
            await trans.CommitAsync();
            return classSchedules
                .Select(sc => _mapper.Map<ClassScheduleResponse>(sc))
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
        var dto = _mapper.Map<UpdateScheduleDto>(request);
        var classSchedule = await _scheduleManagementService.UpdateSchedule(schoolId, dto);
        return _mapper.Map<ClassScheduleResponse>(classSchedule);
    }

    public async Task<ClassSchedulePaginationResponse> GetScheduleView(Guid schoolId, DateOnly date)
    {
        return await _scheduleManagementService.GetScheduleView(schoolId, date);
    }

    public async Task<List<ClassScheduleResponse>> GetScheduleByDate(Guid schoolId, DateOnly date)
    {
        var classSchedules = await _scheduleManagementService.GetScheduleByDate(schoolId, date);

        return classSchedules.Select(sc => MapToClassScheduleResponse(sc))
            .ToList();
    }

    public async Task DeleteSchedule(Guid schoolId, Guid id)
    {
        await _scheduleManagementService.DeleteSchedule(schoolId, id);
    }

    public async Task DeleteSchedule(Guid schoolId, List<Guid> ids)
    {
        var trans = await _context.Database.BeginTransactionAsync();
        try
        {
            await _scheduleManagementService.DeleteSchedule(schoolId, ids);
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
        var response =  _mapper.Map<ClassScheduleResponse>(schedule);
        response.ClassName = schedule.Class.ClassName;
        response.Grade = schedule.Class.Grade;
        response.GradeName = schedule.Class.Grade.GetEnumDisplayName();
        
        return response;
    }
}