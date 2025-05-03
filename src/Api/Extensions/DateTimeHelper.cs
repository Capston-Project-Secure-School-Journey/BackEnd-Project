namespace Api.Extensions;

public static class DateTimeHelper
{
    public static DateTime GetDateTimeUtc7()
    {
        return DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)).DateTime;
    }

    public static string ConvertSecondsToTimeString(int totalSeconds)
    {
        var days = totalSeconds / 86400;
        var remainder = totalSeconds % 86400;
        var hours = remainder / 3600;
        remainder %= 3600;
        var minutes = remainder / 60;

        List<string> components = new();

        if (days > 0)
            components.Add($"{days} ngày");
        if (hours > 0)
            components.Add($"{hours} giờ");
        if (minutes > 0)
            components.Add($"{minutes} phút");

        if (components.Count == 0)
            return "0 phút";
        return string.Join(" ", components);
    }

    public static (DateOnly StartOfWeek, DateOnly EndOfWeek) GetWeekRange(DateOnly date)
    {
        return GetWeekRange(new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc));
    }

    private static (DateOnly StartOfWeek, DateOnly EndOfWeek) GetWeekRange(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;

        var startOfWeek = DateOnly.FromDateTime(date.AddDays(-diff).Date);
        var endOfWeek = startOfWeek.AddDays(6);

        return (startOfWeek, endOfWeek);
    }

    private static (DateOnly StartOfMonth, DateOnly EndOfMonth) GetMonthRange(DateTime date)
    {
        var startOfMonth =
            DateOnly.FromDateTime(new DateTime(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Utc));
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
        return (startOfMonth, endOfMonth);
    }

    public static (DateOnly StartOfMonth, DateOnly EndOfMonth) GetMonthRange(DateOnly date)
    {
        return GetMonthRange(new DateTime(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Utc));
    }
}