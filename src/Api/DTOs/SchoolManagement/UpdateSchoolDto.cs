using Api.Common.Enums;

namespace Api.DTOs.SchoolManagement;

public class UpdateSchoolDto
{
    public Guid Id { get; set; }
    public SchoolType SchoolType { get; set; }
    public string? SchoolDescription { get; set; }
    public string Address { get; set; } = string.Empty;
    public TimeSpan MorningStartTime { get; set; }
    public TimeSpan MorningEndTime { get; set; }
    public TimeSpan AfternoonEndTime { get; set; }
    public TimeSpan AfternoonStartTime { get; set; }
    public string? Email { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
}