using System.ComponentModel.DataAnnotations.Schema;
namespace Api.Domain.Models;
using Api.Common.Enums;

public class User : BaseModel
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserType UserType { get; set; }
    public string UserTypeName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public Gender Gender { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? DetailAddress { get; set; }
    public string? AvatarUrl { get; set; }
    public AccountStatus AccountStatus { get; set; }
}
