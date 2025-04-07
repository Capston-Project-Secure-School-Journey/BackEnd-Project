namespace Api.Extensions;

public static class DateTimeHelper
{
    public static DateTime GetDateTimeUtc7()
    {
        return DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)).DateTime;
    }
}