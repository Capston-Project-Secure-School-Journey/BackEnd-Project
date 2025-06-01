using Api.Common.Enums;

namespace Api.TransferDTOs.Responses;

public class ParentShuttleScheduleResponse
{
    public Guid Id { get; set; }
    public string SchoolName { get; set; } = string.Empty;
    public ShuttleScheduleType Type { get; set; }
    public SessionType SessionType { get; set; }
    public DateOnly Date { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string DriverAvatar { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public Gender DriverGender { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public JourneyStatus JourneyStatus { get; set; }
    public TimeSpan PickupStartTime { get; set; }
    public TimeSpan PickupEndTime { get; set; }
    public TimeSpan? StartJourneyTime { get; set; }
    public TimeSpan? EndJourneyTime { get; set; }
    public double CurrentLat { get; set; } = 0;
    public double CurrentLng { get; set; } = 0;
    public Guid StudentId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public Guid ClassId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public bool IsPickedUp { get; set; } = false;
    public DateTime? PickedUpTime { get; set; } = null;
    public bool IsDroppedOff { get; set; } = false;
    public DateTime? DroppedOffTime { get; set; } = null;
    public bool SkipPickup { get; set; } = false;
    public string IsSkipUpReason { get; set; } = string.Empty;
}