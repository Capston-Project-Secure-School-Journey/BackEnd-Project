namespace Api.Services.NotificationService;

public interface INotificationSender
{
    Task SendOneAsync(string deviceToken, string title, string body, IReadOnlyDictionary<string, string>? data);
    Task SendByNotificationAsync(Guid notificationId, IReadOnlyDictionary<string, string>? data);
    Task SendByNotificationManyAsync(List<Guid> notificationIds, IReadOnlyDictionary<string, string>? data);
    Task SendManyAsync(string[] deviceTokens, string title, string body, IReadOnlyDictionary<string, string>? data);
    Task SendDataAsync(string deviceToken, IReadOnlyDictionary<string, string> data, TimeSpan? timeToLive);
    Task SendDataManyAsync(string[] deviceTokens, IReadOnlyDictionary<string, string> data, TimeSpan? timeToLive);
}