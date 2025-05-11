using Api.Common.Utilities;
using Api.Domain;
using Api.Services.NotificationService;
using Microsoft.EntityFrameworkCore;

namespace Api.Jobs;

// ReSharper disable once ClassNeverInstantiated.Global
public class SendNotificationJob(IServiceProvider serviceProvider
    , ILogger<SendNotificationJob> logger) : IJob
{
    public async Task ExecuteAsync(params object[] args)
    {
        try
        {
            if (args[0] is not List<Guid> notificationIds || notificationIds.Count == 0)
                throw new InvalidDataException(ErrorMessages.InvalidArgumentType);

            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<Context>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var notificationSender = scope.ServiceProvider.GetRequiredService<INotificationSender>();

            foreach (var id in notificationIds)
            {
                var notification = await notificationService.GetNotificationAsync(id);
                var user = await db.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == notification.RecipientId);
                var deviceTokens = user?.DeviceTokens;

                if (deviceTokens == null || deviceTokens.Length == 0)
                    continue;
                await notificationSender.SendManyAsync(deviceTokens, notification.Title, notification.Content);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occured while sending notification");
        }
    }
}