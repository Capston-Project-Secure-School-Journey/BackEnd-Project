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
        } ;
        
        RecurringJob.AddOrUpdate<SchoolJobSubscriber>(
            recurringJobId: "school-subscribe-job",
            methodCall: job => job.ExecuteAsync(),
            cronExpression: "0 2 * * 6",
            options: options);
    }
}