using Api.Common.Enums;

namespace Api.Transfers.Requests;

public class GetClassesRequest: QueryTemplate
{
    public string? ClassName { get; set; }
    public Grade? Grade { get; set; }
}