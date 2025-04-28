namespace Api.Services.NotificationService;

public interface INotificationSender
{
    Task SendAsync(string deviceToken, string title, string body);
    Task SendAsync(string deviceToken, Guid notificationId);
    Task SendAsync(string deviceToken, string title, string body, IReadOnlyDictionary<string, string>? data);
    Task SendAsync(string deviceToken, Guid notificationId, IReadOnlyDictionary<string, string>? data);
    Task SendManyAsync(string[] deviceTokens, string title, string body);
    Task SendManyAsync(string[] deviceTokens, Guid notificationId);
    Task SendManyAsync(string[] deviceTokens, string title, string body, IReadOnlyDictionary<string, string>? data);
    Task SendManyAsync(string[] deviceTokens, Guid notificationId, IReadOnlyDictionary<string, string>? data);
    Task SendDataAsync(string deviceToken, IReadOnlyDictionary<string, string>? data);
    Task SendDataManyAsync(string[] deviceTokens, IReadOnlyDictionary<string, string>? data);
}