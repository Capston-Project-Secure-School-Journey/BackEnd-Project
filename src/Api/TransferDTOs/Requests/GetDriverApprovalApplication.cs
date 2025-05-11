using Api.Common.Enums;

namespace Api.TransferDTOs.Requests;

public class GetDriverApprovalApplication: QueryTemplate
{
    public RequestStatus? Status { get; set; } = null;
}