namespace Api.Common.Enums;

public enum RequestStatus
{
    Created = 0,
    Pending = 1,
    Rejected = 2,
    NeedMoreInfo = 3,
    Approved = 4,
    Cancelled = 5,
    CancellationPending = 6
}