using Api.Common.Enums;

namespace Api.Transfers.Requests;

public class CreateStudentRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public Guid ClassId { get; set; }
    public Gender Gender { get; set; }
}