using Api.Common.Enums;

namespace Api.Domain.Models;

public class User : BaseModel
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public UserType UserType { get; set; }
    public string UserTypeName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public Gender Gender { get; set; }
    public string Email { get; set; } = null!;
    public DateOnly? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? DetailAddress { get; set; }
    public Guid? AvatarKey { get; set; }
    public AccountStatus AccountStatus { get; set; }
    public VerificationMethod? VerificationMethod { get; set; }
    public string[] DeviceTokens { get; set; } = [];
}