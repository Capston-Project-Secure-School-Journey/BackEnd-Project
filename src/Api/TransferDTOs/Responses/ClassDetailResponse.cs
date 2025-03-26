using Api.Common.Enums;

namespace Api.TransferDTOs.Responses;

public class ClassDetailResponse
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public Grade Grade { get; set; }
    public string GradeName { get; set; } = string.Empty;
    public List<ManagedTeacherResponse> ManagedTeachers { get; set; } = null!;
    public string ClassName { get; set; } = string.Empty;
    public int NumberOfStudent { get; set; }
}

public class ManagedTeacherResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}