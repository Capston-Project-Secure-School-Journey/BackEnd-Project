using Api.Common.Enums;

namespace Api.TransferDTOs.Responses;

public class StudentDetailResponse
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public string SchoolName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public string? AvatarUrl { get; set; }
    public string? QrImageUrl { get; set; }
    public string PickUpLocation { get; set; } = string.Empty;
    public decimal PickUpLat { get; set; }
    public decimal PickUpLng { get; set; }
    public bool NeedsPickup { get; set; }
}