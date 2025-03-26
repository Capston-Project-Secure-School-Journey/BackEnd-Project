using Api.Attributes;

namespace Api.Common.Enums;

public enum BanType
{
    [BanAttemptLimit(5, 1800, 1)]
    Login,
    [BanAttemptLimit(50, 1800, 1)]
    S3PreSigned,
    [BanAttemptLimit(5, 86400, 24)]
    SendVerifyEmail,
    [BanAttemptLimit(5, 86400, 24)]
    SendSms,
    [BanAttemptLimit(5, 86400, 24)]
    AddChild
}