using Api.Common.Enums;
using Api.Domain.Models;

namespace Api.DTOs.ClassManagement;

public class CreateClassDto
{
    public Guid SchoolId { get; set; }
    public Grade Grade { get; set; }
    public List<ManagedTeacher> ManagedTeachers { get; init; } = [];
    public string ClassName { get; init; } = string.Empty;
}