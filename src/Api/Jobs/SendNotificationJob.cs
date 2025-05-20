using Api.Common.Utilities;
using Api.Services.NotificationService;

namespace Api.Jobs;

// ReSharper disable once ClassNeverInstantiated.Global
public class SendNotificationJob(IServiceProvider serviceProvider, ILogger<SendNotificationJob> logger) : IJob
{
    public async Task ExecuteAsync(params object[] args)
    {
        try
        {
            if (args[0] is not List<Guid> notificationIds || notificationIds.Count == 0)
                throw new InvalidDataException(ErrorMessages.InvalidArgumentType);
            var data = args[1] as Dictionary<string, string>;
            
            using var scope = serviceProvider.CreateScope();
            var notificationSender = scope.ServiceProvider.GetRequiredService<INotificationSender>();

            await notificationSender.SendByNotificationManyAsync(notificationIds, data);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occured while sending notification");
        }
    }
}