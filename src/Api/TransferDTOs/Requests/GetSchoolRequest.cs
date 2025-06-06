using Api.Common.Enums;

namespace Api.TransferDTOs.Requests;

public class GetSchoolRequest : QueryTemplate
{
    public SchoolType? SchoolType { get; set; } = null;
    public string? SchoolName { get; set; } = null;
}