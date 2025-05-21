using Api.Extensions;
using Api.Jobs.ShuttleScheduleJobs;
using Hangfire;

namespace Api.Jobs;

public static class JobRegister
{
    public static void Register()
    {
        var options = new RecurringJobOptions()
        {
            TimeZone = TimeZoneHelper.VietnamTimeZone
        };

        RecurringJob.AddOrUpdate<SchoolJobSubscriber>(
            "school-subscribe-job",
            job => job.ExecuteAsync(),
            "0 2 * * 6",
            options);

        RecurringJob.AddOrUpdate<CleanFileJob>(
            "daily-cleanup-job",
            job => job.ExecuteAsync(),
            "35 2 * * *",
            options);
    }
}