using Api.Domain;
using Api.DTOs.ScheduleManagement;
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
            await trans.RollbackAsync();
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
}