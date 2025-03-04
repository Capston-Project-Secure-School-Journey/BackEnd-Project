using Api.Common.Enums;
using Api.Domain.Models;

namespace Api.DTOs.ClassManagement;

public class UpdateClassDto
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public Grade Grade { get; init; }
    public List<ManagedTeacher> ManagedTeachers { get; init; } = [];
    public string ClassName { get; init; } = string.Empty;
}