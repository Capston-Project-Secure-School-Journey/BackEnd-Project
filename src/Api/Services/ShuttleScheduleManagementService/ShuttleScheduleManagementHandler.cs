using Api.Domain.Models;
using Api.TransferDTOs.Responses;

namespace Api.Services.ShuttleScheduleManagementService;

public class ShuttleScheduleManagementHandler(IShuttleScheduleManagementService shuttleScheduleManagement)
    : IShuttleScheduleManagementHandler
{
    public async Task<ShuttleScheduleView> GetShuttleScheduleView(DateOnly date, Guid schoolId)
    {
        return await shuttleScheduleManagement.GetShuttleScheduleView(date, schoolId);
    }

    public async Task<List<ShuttleScheduleResponse>> GetShuttleScheduleByDate(DateOnly date, Guid schoolId)
    {
        return await shuttleScheduleManagement.GetShuttleScheduleByDate(date, schoolId);
    }

    public async Task<ShuttleSchedule> GetShuttleSchedule(Guid shuttleScheduleId, Guid schoolId)
    {
        await shuttleScheduleManagement.IsOwnerOfShuttleSchedule(shuttleScheduleId, schoolId);
        return await shuttleScheduleManagement.GetShuttleSchedule(shuttleScheduleId);
    }
}