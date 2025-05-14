using Api.Common.Enums;

namespace Api.TransferDTOs.Responses;

public class PickupScheduleResponse
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public string SchoolName { get; set; } = string.Empty;
    public SessionType SessionType { get; set; }
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
}