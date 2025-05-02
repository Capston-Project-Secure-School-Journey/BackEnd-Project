namespace Api.Extensions;

public static class DateTimeHelper
{
    public static DateTime GetDateTimeUtc7()
    {
        return DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)).DateTime;
    }

    public static string ConvertSecondsToTimeString(int totalSeconds)
    {
        int days = totalSeconds / 86400;
        int remainder = totalSeconds % 86400;
        int hours = remainder / 3600;
        remainder %= 3600;
        int minutes = remainder / 60;

        List<string> components = new List<string>();

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
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;

        DateOnly startOfWeek = DateOnly.FromDateTime(date.AddDays(-diff).Date);
        DateOnly endOfWeek = startOfWeek.AddDays(6);

        return (startOfWeek, endOfWeek);
    }

    private static (DateOnly StartOfMonth, DateOnly EndOfMonth) GetMonthRange(DateTime date)
    {
        DateOnly startOfMonth =
            DateOnly.FromDateTime(new DateTime(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Utc));
        DateOnly endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
        return (startOfMonth, endOfMonth);
    }

    public static (DateOnly StartOfMonth, DateOnly EndOfMonth) GetMonthRange(DateOnly date)
    {
        return GetMonthRange(new DateTime(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Utc));
    }
}