using Api.Common.Enums;

namespace Api.Domain.Models;

public class PickupSchedule
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public string SchoolName { get; set; } = string.Empty;
    public SessionType SessionType { get; set; }
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public Guid DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string DriverAvatar { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public Gender DriverGender { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public bool IsAllNotesRead { get; set; }
    public JourneyStatus JourneyStatus { get; set; }
    public int NumberOfStudents { get; set; }
    public int NumberOfCurrentStudents { get; set; }
    public Dictionary<string, string> BestRoute { get; set; } = new();
    public List<StudentOnBus> Students { get; set; } = new();
}