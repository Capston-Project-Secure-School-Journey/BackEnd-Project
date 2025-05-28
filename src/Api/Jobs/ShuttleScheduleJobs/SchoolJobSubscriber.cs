using Api.Domain;
using Api.Extensions;
using Hangfire;
using Microsoft.EntityFrameworkCore;

// ReSharper disable All

namespace Api.Jobs.ShuttleScheduleJobs;

public class SchoolJobSubscriber(IServiceProvider serviceProvider) : IJob
{
    public async Task ExecuteAsync(params object[] args)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Context>();

        var schoolIds = await db.Schools.Select(s => s.Id).ToListAsync();

        var dates = DateTimeHelper.Get246DatesOfCurrentWeek();
        var saturdayNextWeek = DateTimeHelper.GetSaturdayOfCurrentWeek();

        foreach (var schoolId in schoolIds)
        {
            foreach (var date in dates.Select(dt =>
                         new DateTimeOffset(dt.Year, dt.Month, dt.Day, 9, 0, 0, TimeSpan.FromHours(7))))
            {
                BackgroundJob.Schedule<AlertMissingAddressJob>(
                    job => job.ExecuteAsync(schoolId),
                    date);

                BackgroundJob.Schedule<AlertInsufficientDriversJob>(
                    job => job.ExecuteAsync(schoolId),
                    date);
            }

            BackgroundJob.Schedule<CreateShuttleScheduleJob>(
                job => job.ExecuteAsync(schoolId),
                new DateTimeOffset(saturdayNextWeek.Year, saturdayNextWeek.Month, saturdayNextWeek.Day, 9, 0, 0,
                    TimeSpan.FromHours(7)));
        }
    }
}