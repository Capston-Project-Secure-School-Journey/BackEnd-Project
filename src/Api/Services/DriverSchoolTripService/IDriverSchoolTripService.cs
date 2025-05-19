using Api.Domain.Models;
using Api.TransferDTOs.Responses;

namespace Api.Services.DriverSchoolTripService;

public interface IDriverSchoolTripService
{
    Task<List<ShuttleScheduleResponse>> GetShuttleScheduleByDate(Guid driverId, DateOnly date);
    Task StartJourney(Guid shuttleScheduleId);
    Task EndJourney(Guid shuttleScheduleId);
    Task CancelJourney(Guid shuttleScheduleId, string cancelReason);
    Task SkipStudent(Guid shuttleScheduleId, Guid studentId, string cancelReason);
    Task<bool> HasInProgressShuttle(Guid driverId);
    Task<ShuttleSchedule> GetCurrentShuttleScheduleByDriver(Guid driverId);
    Task<bool> HasUpcomingShuttle(Guid driverId);
    Task<ShuttleSchedule> GetUpcomingShuttleSchedule(Guid driverId);
    Task IsOwnerOfShuttleSchedule(Guid shuttleScheduleId, Guid driverId);
    Task UpdateCurrentAddress(Guid shuttleScheduleId, Guid driveId, double lat, double lng);
}