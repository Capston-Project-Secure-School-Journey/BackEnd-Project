using Api.Common.Enums;

namespace Api.DTOs.User;

public class UpdateUserInfoDto
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? DetailAddress { get; set; }
}