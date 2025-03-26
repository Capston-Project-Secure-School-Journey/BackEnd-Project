using Api.Common.Enums;

namespace Api.TransferDTOs.Responses;

public class ClassResponse
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public Grade Grade { get; set; }
    public string GradeName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public int NumberOfStudent { get; set; }
}