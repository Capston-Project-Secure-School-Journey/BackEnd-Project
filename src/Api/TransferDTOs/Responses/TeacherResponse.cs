using Api.Common.Enums;

namespace Api.TransferDTOs.Responses;

public class TeacherResponse
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
}