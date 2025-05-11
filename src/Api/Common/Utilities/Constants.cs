using Api.Common.Enums;
using Api.Extensions;

namespace Api.Common.Utilities;

public static class Constants
{
    #region Hash Key

    private const string StudentHashKey = $"StudentId_";

    public static string GetStudentStringToHash(Guid studentId)
    {
        return StudentHashKey + studentId;
    }

    #endregion

    #region ColumnTypes

    private const string VarcharFormat = "varchar({0})";
    private const string NvarcharFormat = "nvarchar({0})";
    private const string CharFormat = "char({0})";
    private const string DecimalFormat = "decimal({0},{1})";

    public const string IntUnsigned = "int unsigned";
    public const string Datetime = "datetime";
    public const string Date = "date";
    public const string Time = "time";
    public const string Bit = "bit";
    public const string Json = "json";
    public const string Tinyint = "tinyint";
    public const string Float = "float";
    public const string Double = "double";
    public const string Timestamp = "timestamp";

    // Helper methods
    public static string Varchar(int length)
    {
        return string.Format(VarcharFormat, length);
    }

    public static string Nvarchar(int length)
    {
        return string.Format(NvarcharFormat, length);
    }

    public static string Char(int length)
    {
        return string.Format(CharFormat, length);
    }

    public static string Decimal(int precision, int scale)
    {
        return string.Format(DecimalFormat, precision, scale);
    }

    #endregion

    public static string GetReason(BanType type)
    {
        var banTime = type.GetBanAttemptBanTime();
        var limit = type.GetBanAttemptLimit();

        switch (type)
        {
            case BanType.Login:
                return
                    $"Bạn đã đăng nhập sai quá {limit} lần. Hãy đợi sau {DateTimeHelper.ConvertSecondsToTimeString(banTime)} để đăng nhập lại";
            case BanType.S3PreSigned:
                return
                    $"Bạn đã quá giới hạn tải file. Hãy đợi sau {DateTimeHelper.ConvertSecondsToTimeString(banTime)} để thử lại";
            case BanType.SendVerifyEmail:
                return
                    $"Bạn đã yêu cầu gửi email quá {limit} lần. Hãy đợi sau {DateTimeHelper.ConvertSecondsToTimeString(banTime)} để thử lại";
            case BanType.SendSms:
                return
                    $"Bạn đã yêu cầu gửi tin nhắn quá {limit} lần. Hãy đợi sau {DateTimeHelper.ConvertSecondsToTimeString(banTime)} để thử lại";
            case BanType.AddChild:
                return
                    $"Bạn đã sai quá {limit} lần khi xác thực thông tin. Hãy đợi sau {DateTimeHelper.ConvertSecondsToTimeString(banTime)} để thử lại";
            default:
                return "Bạn đã bị ban";
        }
    }
}