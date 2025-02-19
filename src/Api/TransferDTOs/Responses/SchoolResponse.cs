using Api.Common.Enums;

namespace Api.Transfers.Responses;

public class SchoolResponse
{
    public Guid Id { get; set; }
    public SchoolType SchoolType { get; set; }
    public string SchoolName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
}