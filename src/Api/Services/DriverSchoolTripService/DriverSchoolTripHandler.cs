using Api.Domain.Models;
using Api.Services.ShuttleScheduleManagementService;
using Api.TransferDTOs.Responses;

namespace Api.Services.DriverSchoolTripService;

public class DriverSchoolTripHandler(
    IDriverSchoolTripService driverSchoolTripService,
    IShuttleScheduleManagementService shuttleScheduleManagementService) : IDriverSchoolTripHandler
{
    public async Task<List<ShuttleScheduleResponse>> GetShuttleScheduleByDate(Guid driverId, DateOnly date)
    {
        return await driverSchoolTripService.GetShuttleScheduleByDate(driverId, date);
    }

    public async Task<ShuttleSchedule> GetShuttleSchedule(Guid shuttleScheduleId, Guid driverId)
    {
        await driverSchoolTripService.IsOwnerOfShuttleSchedule(shuttleScheduleId, driverId);
        return await shuttleScheduleManagementService.GetShuttleSchedule(shuttleScheduleId);
    }

    public async Task StartJourney(Guid shuttleScheduleId, Guid driverId)
    {
        await driverSchoolTripService.IsOwnerOfShuttleSchedule(shuttleScheduleId, driverId);
        await driverSchoolTripService.StartJourney(shuttleScheduleId);
    }

    public async Task EndJourney(Guid shuttleScheduleId, Guid driverId)
    {
        await driverSchoolTripService.IsOwnerOfShuttleSchedule(shuttleScheduleId, driverId);
        await driverSchoolTripService.EndJourney(shuttleScheduleId);
    }

    public async Task CancelJourney(Guid shuttleScheduleId, Guid driverId, string cancelReason)
    {
        await driverSchoolTripService.IsOwnerOfShuttleSchedule(shuttleScheduleId, driverId);
        await driverSchoolTripService.CancelJourney(shuttleScheduleId, cancelReason);
    }

    public async Task SkipStudent(Guid shuttleScheduleId, Guid driverId, Guid studentId, string cancelReason)
    {
        await driverSchoolTripService.IsOwnerOfShuttleSchedule(shuttleScheduleId, driverId);
        await driverSchoolTripService.SkipStudent(shuttleScheduleId, studentId, cancelReason);
    }

    public async Task<bool> HasInProgressShuttle(Guid driverId)
    {
        return await driverSchoolTripService.HasInProgressShuttle(driverId);
    }

    public async Task<ShuttleSchedule> GetCurrentShuttleScheduleByDriver(Guid driverId)
    {
        return await driverSchoolTripService.GetCurrentShuttleScheduleByDriver(driverId);
    }

    public async Task<bool> HasUpcomingShuttle(Guid driverId)
    {
        return await driverSchoolTripService.HasUpcomingShuttle(driverId);
    }

    public async Task<ShuttleSchedule> GetUpcomingShuttleSchedule(Guid driverId)
    {
        return await driverSchoolTripService.GetUpcomingShuttleSchedule(driverId);
    }

    public async Task UpdateCurrentAddress(Guid shuttleScheduleId, Guid driverId, double lat, double lng)
    {
        await driverSchoolTripService.UpdateCurrentAddress(shuttleScheduleId, driverId, lat, lng);
    }
}