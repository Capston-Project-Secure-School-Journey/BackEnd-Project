using Api.Domain.Models;
using Api.DTOs.ShuttleScheduleService;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;
using MongoDB.Driver;

namespace Api.Services.ShuttleScheduleManagementService;

public interface IShuttleScheduleManagementService
{
    Task UpdateShuttleSchedule(ShuttleSchedule shuttleSchedule);
    Task UpdateStudentOnShuttleSchedule(Guid shuttleScheduleId, StudentOnBus studentOnBus);
    Task<List<ShuttleSchedule>> AddShuttleSchedule(List<CreateShuttleScheduleDto> requests);
    Task DeleteShuttleSchedule(Guid schoolId, DateOnly startDate, DateOnly endDate);
    Task<ShuttleScheduleView> GetShuttleScheduleView(Guid schoolId, DateOnly date);

    Task<IFindFluent<ShuttleSchedule, ShuttleSchedule>> GetShuttleScheduleByDate(Guid schoolId,
        GetShuttleScheduleByDateRequest request);

    Task<ShuttleSchedule> GetShuttleSchedule(Guid shuttleScheduleId);
    Task IsOwnerOfShuttleSchedule(Guid schoolId, Guid shuttleScheduleId);
}