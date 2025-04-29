using Api.Common.Enums;
using Api.Extensions;

namespace Api.DTOs.ApprovalProcessor;

public class ApplicationActionDto
{
    public ApplicationAction Action { get; set; }
    public string ActionName => Action.GetEnumDisplayName();
}