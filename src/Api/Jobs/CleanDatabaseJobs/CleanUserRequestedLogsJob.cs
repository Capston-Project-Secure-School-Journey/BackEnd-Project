using System.Reflection;
using Api.Attributes;
using Api.Common.Enums;
using Api.Domain;
using Api.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Api.Jobs.CleanDatabaseJobs;

public class CleanUserRequestedLogsJob(
    Context context,
    ILogger<CleanUserRequestedLogsJob> logger) : IJob
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2139",
        Justification = "Handled and logged in job context")]
    public async Task ExecuteAsync(params object[] args)
    {
        IDbContextTransaction? trans = null;
        var maxObservationWindow = GetMaxObservationWindow();
        var now = DateTimeHelper.GetDateTimeUtc7();
        var thresholdTime = now.AddHours(-maxObservationWindow);
        try
        {
            context.BypassSoftDelete = true;
            var requestedLogs = await context
                .UserRequestedLogs
                .IgnoreQueryFilters()
                .Where(rq => rq.DatetimeRequested < thresholdTime)
                .ToListAsync();

            if (requestedLogs.Count == 0)
            {
                logger.LogInformation("Clean user requested logs successfully");
                logger.LogInformation("No requested logs deleted");
                return;
            }

            trans = await context.Database.BeginTransactionAsync();
            context.UserRequestedLogs.RemoveRange(requestedLogs);
            await context.SaveChangesAsync();
            await trans.CommitAsync();
            logger.LogInformation("Clean user requested logs successfully");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Clean user requested logs failed");
            if (trans != null)
                await trans.DisposeAsync();
        }
    }

    private static int GetMaxObservationWindow()
    {
        return Enum.GetValues(typeof(BanType))
            .Cast<BanType>()
            .Select(bt =>
            {
                var field = typeof(BanType).GetField(bt.ToString());
                var attr = field?.GetCustomAttribute<BanAttemptLimitAttribute>();
                return attr?.ObservationWindow ?? 24;
            })
            .Max();
    }
}