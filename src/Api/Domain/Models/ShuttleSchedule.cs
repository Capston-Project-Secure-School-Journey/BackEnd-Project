using Api.Common.Enums;
using Api.Services;

namespace Api.Domain.Models;

public class ShuttleSchedule
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public string SchoolName { get; set; } = string.Empty;
    public ShuttleScheduleType Type { get; set; }
    public SessionType SessionType { get; set; }
    public DateOnly Date { get; set; }
    public Guid DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string DriverAvatar { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public Gender DriverGender { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public bool IsAllNotesRead { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public JourneyStatus JourneyStatus { get; set; }
    public string CancelReason { get; set; } = string.Empty;
    public int NumberOfStudents { get; set; }
    public int NumberOfPickedUpStudents { get; set; }
    public int NumberOfDroppedOffStudents { get; set; }
    public double CurrentLat { get; set; } = 0;
    public double CurrentLng { get; set; } = 0;
    public string BestRoute { get; set; } = string.Empty;
    public List<StudentOnBus> Students { get; set; } = new();
}