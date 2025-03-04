using Api.Common.Enums;

namespace Api.TransferDTOs.Responses;

public class TeacherResponse
{
    public Guid Id { get; init; }
    public Guid SchoolId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public DateOnly DateOfBirth { get; init; }
    public Gender Gender { get; init; }
    public string PhoneNumber { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}