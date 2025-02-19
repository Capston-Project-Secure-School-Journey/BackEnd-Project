using System.ComponentModel.DataAnnotations;
using Api.Common.Enums;

namespace Api.DTOs.UserManagement;

public class CreateSchoolAdminDto
{
    public Guid SchoolId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}