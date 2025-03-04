using Api.Common.Enums;

namespace Api.TransferDTOs.Responses;

public class SchoolDetailResponse
{
    public Guid Id { get; set; }
    public SchoolType SchoolType { get; set; }
    public string SchoolName { get; set; } = string.Empty;
    public string? SchoolDescription { get; set; }
    public string Address { get; set; } = string.Empty;
    public TimeSpan MorningStartTime { get; set; }
    public TimeSpan MorningEndTime { get; set; }
    public TimeSpan AfternoonEndTime { get; set; }
    public TimeSpan AfternoonStartTime { get; set; }
    public string? Email { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;

    public string? SchoolAdminUserName { get; set; }
    public List<string> Images { get; set; } = [];
}