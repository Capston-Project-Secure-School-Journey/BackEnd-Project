using System.Runtime.InteropServices;

namespace Api.Extensions;

public static class TimeZoneHelper
{
    public static TimeZoneInfo VietnamTimeZone =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
            : TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
}