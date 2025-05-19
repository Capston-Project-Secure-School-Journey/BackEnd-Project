using Api.Domain.Models;
using Api.TransferDTOs.Responses;

namespace Api.Services.DriverSchoolTripService;

public interface IDriverSchoolTripHandler
{
    Task<List<ShuttleScheduleResponse>> GetShuttleScheduleByDate(Guid driverId, DateOnly date);
    Task<ShuttleSchedule> GetShuttleSchedule(Guid shuttleScheduleId, Guid driverId);
    Task StartJourney(Guid shuttleScheduleId, Guid driverId);
    Task EndJourney(Guid shuttleScheduleId, Guid driverId);
    Task CancelJourney(Guid shuttleScheduleId, Guid driverId, string cancelReason);
    Task SkipStudent(Guid shuttleScheduleId, Guid driverId, Guid studentId, string cancelReason);
    Task<bool> HasInProgressShuttle(Guid driverId);
    Task<ShuttleSchedule> GetCurrentShuttleScheduleByDriver(Guid driverId);
    Task<bool> HasUpcomingShuttle(Guid driverId);
    Task<ShuttleSchedule> GetUpcomingShuttleSchedule(Guid driverId);
    Task UpdateCurrentAddress(Guid shuttleScheduleId, Guid driverId, double lat, double lng);
}