using Api.Common.Enums;

namespace Api.Transfers.Requests;

public class GetSchoolRequest: QueryTemplate
{
    public SchoolType? SchoolType { get; set; }
    public string? SchoolName { get; set; } = string.Empty;
}