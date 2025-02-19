namespace Api.Common.Enums;

public class BannedUser
{
    public Guid Id { get; set; }
    public UserRequestedType UserRequestedType { get; set; }
    public DateTime DatetimeBanned { get; set; }
    public long DatetimeUnban { get; set; }
    public string Reason { get; set; } = string.Empty;
}