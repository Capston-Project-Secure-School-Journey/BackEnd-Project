using Api.Domain;
using Api.Extensions;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace Api.Jobs;

public class SchoolJobSubscriber : IJob
{
    private readonly IServiceProvider _serviceProvider;

    public SchoolJobSubscriber(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(params object[] args)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Context>();

        var schoolIds = await db.Schools.Select(s => s.Id).ToListAsync();

        var dates = GetNextWeek246Dates();
        var saturdayNextWeek = GetSaturdayNextWeek();

        foreach (var schoolId in schoolIds)
        {
            foreach (var delay in dates.Select(runDate => runDate.Date.AddHours(9))
                         .Select(runAt => runAt - DateTimeHelper.GetDateTimeUtc7()))
                BackgroundJob.Schedule<AlertMissingAddressJob>(
                    job => job.ExecuteAsync(schoolId),
                    delay);

            BackgroundJob.Schedule<CreatePickupScheduleJob>(
                job => job.ExecuteAsync(schoolId),
                saturdayNextWeek - DateTimeHelper.GetDateTimeUtc7());
        }
    }

    private static List<DateTime> GetNextWeek246Dates()
    {
        var today = DateTimeHelper.GetDateTimeUtc7();

        var daysUntilNextMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
        var nextMonday = today.AddDays(daysUntilNextMonday + 7);

        return
        [
            nextMonday,
            nextMonday.AddDays(2),
            nextMonday.AddDays(4)
        ];
    }

    private static DateTime GetSaturdayNextWeek()
    {
        var today = DateTimeHelper.GetDateTimeUtc7();
        var daysUntilNextMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
        var nextMonday = today.AddDays(daysUntilNextMonday + 7);

        return nextMonday.AddDays(5);
    }
}