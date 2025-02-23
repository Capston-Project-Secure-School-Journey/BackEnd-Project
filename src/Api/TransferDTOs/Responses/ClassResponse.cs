using Api.Common.Enums;
using Api.Domain.Models;

namespace Api.TransferDTOs.Responses;

public class ClassResponse
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public Grade Grade { get; set; }
    public List<ManagedTeacherResponse> ManagedTeachers { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public int NumberOfStudent { get; set; }
}

public class ManagedTeacherResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}
