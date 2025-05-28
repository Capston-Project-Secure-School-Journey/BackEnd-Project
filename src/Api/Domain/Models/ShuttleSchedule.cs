using Api.Common.Enums;

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
    public TimeSpan PickupStartTime { get; set; }
    public TimeSpan PickupEndTime { get; set; }
    public TimeSpan? StartJourneyTime { get; set; }
    public TimeSpan? EndJourneyTime { get; set; }
    public JourneyStatus JourneyStatus { get; set; }
    public string CancelReason { get; set; } = string.Empty;
    public int NumberOfStudents { get; set; }
    public int NumberOfPickedUpStudents { get; set; }
    public int NumberOfDroppedOffStudents { get; set; }
    public double CurrentLat { get; set; } = 0;
    public double CurrentLng { get; set; } = 0;
    public BestRoute BestRoute { get; set; } = null!;
    public List<StudentOnBus> Students { get; set; } = new();
}

public class BestRoute
{
    public Point Origin { get; set; } = null!;
    public Point Destination { get; set; } = null!;
    public List<Point> WayPoints { get; set; } = new();
}

public class Point
{
    public string FullAddress { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}