using Api.Common.Enums;

namespace Api.Domain.Models;

public class UserRequestedLog
{
    public uint Id { get; set; }
    public Guid UserId { get; set; }
    public BanType UserRequestedType { get; set; }
    public DateTime DatetimeRequested { get; set; }
}