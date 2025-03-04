using Api.Common.Enums;

namespace Api.DTOs.ChildrenManagement;

public class RegisterChildDto
{
    public string SecretCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public Relationship Relationship { get; set; }
}