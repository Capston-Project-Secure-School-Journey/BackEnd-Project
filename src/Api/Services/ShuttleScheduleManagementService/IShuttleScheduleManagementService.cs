using Api.Domain.Models;
using Api.DTOs.ShuttleScheduleService;
using Api.TransferDTOs.Responses;

namespace Api.Services.ShuttleScheduleManagementService;

public interface IShuttleScheduleManagementService
{
    Task<ShuttleSchedule> AddShuttleSchedule(CreateShuttleScheduleDto request);
    Task UpdateShuttleSchedule(ShuttleSchedule shuttleSchedule);
    Task UpdateStudentOnShuttleSchedule(Guid shuttleScheduleId, StudentOnBus studentOnBus);
    Task<List<ShuttleSchedule>> AddShuttleSchedule(List<CreateShuttleScheduleDto> requests);
    Task<ShuttleScheduleView> GetShuttleScheduleView(DateOnly date, Guid schoolId);
    Task<List<ShuttleScheduleResponse>> GetShuttleScheduleByDate(DateOnly date, Guid schoolId);
    Task<ShuttleSchedule> GetShuttleSchedule(Guid shuttleScheduleId);
    Task IsOwnerOfShuttleSchedule(Guid shuttleScheduleId, Guid schoolId);
}