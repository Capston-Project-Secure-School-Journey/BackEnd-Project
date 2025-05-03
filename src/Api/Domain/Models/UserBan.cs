using Api.Common.Enums;

namespace Api.Domain.Models;

public class UserBan
{
    public uint Id { get; set; }
    public BanType BanType { get; set; }
    public Guid UserId { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string Reason { get; set; } = string.Empty;
    public DateTime BanDate { get; set; }
    public DateTime BanExpiryDate { get; set; }
}