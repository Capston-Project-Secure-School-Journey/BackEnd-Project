using Api.Domain.Models;
using Api.TransferDTOs.Responses;

namespace Api.Services.ShuttleScheduleManagementService;

public interface IShuttleScheduleManagementHandler
{
    Task<ShuttleScheduleView> GetShuttleScheduleView(DateOnly date, Guid schoolId);
    Task<List<ShuttleScheduleResponse>> GetShuttleScheduleByDate(DateOnly date, Guid schoolId);
    Task<ShuttleSchedule> GetShuttleSchedule(Guid shuttleScheduleId, Guid schoolId);
}