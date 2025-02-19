using Api.Common.Enums;

namespace Api.Domain.Models;

public class UserRequestedLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public UserRequestedType UserRequestedType { get; set; }
    public DateTime DatetimeRequesed { get; set; }
}