using System.ComponentModel.DataAnnotations;
using Api.Common.Enums;

namespace Api.DTOs.UserManagement;

public class CreateSchoolAdminDto
{
    [Required]
    public string Password { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    [Required]
    public string Email { get; set; } = string.Empty;
}