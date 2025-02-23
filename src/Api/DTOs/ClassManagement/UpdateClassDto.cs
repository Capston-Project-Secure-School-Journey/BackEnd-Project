using Api.Common.Enums;
using Api.Domain.Models;

namespace Api.DTOs.ClassManagement;

public class UpdateClassDto
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public Grade Grade { get; set; }
    public List<ManagedTeacher> ManagedTeachers { get; set; }
    public string ClassName { get; set; } = string.Empty;
}