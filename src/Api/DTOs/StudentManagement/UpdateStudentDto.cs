using Api.Common.Enums;

namespace Api.DTOs.StudentManagement;

public class UpdateStudentDto
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public Guid ClassId { get; set; }
    public Gender Gender { get; set; }
}