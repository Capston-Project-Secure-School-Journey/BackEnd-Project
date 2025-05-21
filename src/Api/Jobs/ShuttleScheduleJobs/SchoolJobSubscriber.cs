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

        var dates = DateTimeHelper.GetNextWeek246Dates();
        var saturdayNextWeek = DateTimeHelper.GetSaturdayNextWeek();

        foreach (var schoolId in schoolIds)
        {
            foreach (var delay in dates.Select(runDate => runDate.Date.AddHours(9))
                         .Select(runAt => runAt - DateTimeHelper.GetDateTimeUtc7()))
            {
                BackgroundJob.Schedule<AlertMissingAddressJob>(
                    job => job.ExecuteAsync(schoolId),
                    delay);

                BackgroundJob.Schedule<AlertInsufficientDriversJob>(
                    job => job.ExecuteAsync(schoolId),
                    delay);
            }

            BackgroundJob.Schedule<CreateShuttleScheduleJob>(
                job => job.ExecuteAsync(schoolId),
                saturdayNextWeek.Date.AddHours(23) - DateTimeHelper.GetDateTimeUtc7());
        }
    }
}