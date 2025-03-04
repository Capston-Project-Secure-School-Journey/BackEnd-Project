using Api.Common.Enums;

namespace Api.DTOs.UserManagement;

public class CreateUserDto
{
    public UserType UserType { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Gender Gender { get; set; }
}