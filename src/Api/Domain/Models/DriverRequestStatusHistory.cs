using Api.Common.Enums;

namespace Api.Domain.Models;

public class DriverRequestStatusHistory
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public RequestStatus? FromStatus { get; set; }
    public RequestStatus ToStatus { get; set; }
    public Guid ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
    public string Note { get; set; } = string.Empty;
    public DriverApprovalRequest Request { get; set; } = null!;
}