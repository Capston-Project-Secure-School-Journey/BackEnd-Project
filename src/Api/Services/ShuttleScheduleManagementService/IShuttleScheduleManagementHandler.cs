using Api.Domain.Models;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;

namespace Api.Services.ShuttleScheduleManagementService;

public interface IShuttleScheduleManagementHandler
{
    Task<ShuttleScheduleView> GetShuttleScheduleView(Guid schoolId, DateOnly date);

    Task<Pagination<ShuttleScheduleResponse>> GetShuttleScheduleByDate(Guid schoolId,
        GetShuttleScheduleByDateRequest request);

    Task<ShuttleSchedule> GetShuttleSchedule(Guid schoolId, Guid shuttleScheduleId);
}