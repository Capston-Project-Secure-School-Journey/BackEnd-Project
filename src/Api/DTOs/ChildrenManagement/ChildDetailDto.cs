using Api.Common.Enums;

namespace Api.DTOs.ChildrenManagement;

public class ChildDetailDto
{
    public Guid Id { get; set; }
    public string SchoolName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public string AvatarUrl { get; set; } = string.Empty;
    public string PickUpLocation { get; set; } = string.Empty;
    public decimal PickUpLat { get; set; }
    public decimal PickUpLng { get; set; }
}