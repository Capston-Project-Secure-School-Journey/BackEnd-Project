using Api.Common.Enums;

namespace Api.DTOs.TeacherManagement;

public class UpdateTeacherDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; }  = string.Empty;
}