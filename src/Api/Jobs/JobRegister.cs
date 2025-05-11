using Api.Extensions;
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
        
        // BackgroundJob.Enqueue<CreatePickupScheduleJob>(job => job.ExecuteAsync(new Guid("08dd51bb-21d2-4413-83df-5482d1645010")));
    }
}