using Api.Domain;
using Api.Services.NotificationService;
using Microsoft.EntityFrameworkCore;

namespace Api.Jobs;

public class SendNotificationJob : IJob
{
    private readonly IServiceProvider _serviceProvider;
    public SendNotificationJob(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public async Task ExecuteAsync(params object[] args)
    {
        if (args[0] is not List<Guid> notificationIds || notificationIds.Count == 0)
            throw new InvalidDataException("Invalid argument type");

        using var scope = _serviceProvider.CreateScope();
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
}