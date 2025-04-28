using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace Api.Services.NotificationService;

public class NotificationFcmSender : INotificationSender
{
    private readonly INotificationService _notificationService;
    private readonly FirebaseMessaging _messaging;

    public NotificationFcmSender(IConfiguration config, INotificationService notificationService)
    {
        _notificationService = notificationService;

        if (FirebaseApp.DefaultInstance == null)
        {
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(config["Fcm:CredentialsPath"]!)
            });
        }

        _messaging = FirebaseMessaging.DefaultInstance;
    }

    public async Task SendAsync(string deviceToken, string title, string body)
    {
        await SendAsync(deviceToken, title, body, null);
    }

    public Task SendAsync(string deviceToken, Guid notificationId)
    {
        throw new NotImplementedException();
    }

    public async Task SendAsync(string deviceToken, string title, string body,
        IReadOnlyDictionary<string, string>? data)
    {
        var message = GetMessage(deviceToken, title, body, data);
        var result = await _messaging.SendAsync(message);

        ThrowIfSentFailed(result);
    }

    public async Task SendAsync(string deviceToken, Guid notificationId, IReadOnlyDictionary<string, string>? data)
    {
        var message = await GetMessageAsync(deviceToken, notificationId, data);
        var result = await _messaging.SendAsync(message);

        ThrowIfSentFailed(result);
    }

    public async Task SendManyAsync(string[] deviceTokens, string title, string body)
    {
        var messages = GetMessage(deviceTokens, title, body, null);
        var result = await _messaging.SendEachAsync(messages);

        ThrowIfSentFailed(result);
    }

    public async Task SendManyAsync(string[] deviceTokens, Guid notificationId)
    {
        var messages = await GetMessageAsync(deviceTokens, notificationId, null);
        var result = await _messaging.SendEachAsync(messages);

        ThrowIfSentFailed(result);
    }

    public async Task SendManyAsync(string[] deviceTokens, string title, string body,
        IReadOnlyDictionary<string, string>? data)
    {
        var messages = GetMessage(deviceTokens, title, body, data);
        var result = await _messaging.SendEachAsync(messages);

        ThrowIfSentFailed(result);
    }

    public async Task SendManyAsync(string[] deviceTokens, Guid notificationId,
        IReadOnlyDictionary<string, string>? data)
    {
        var messages = await GetMessageAsync(deviceTokens, notificationId, data);
        var result = await _messaging.SendEachAsync(messages);

        ThrowIfSentFailed(result);
    }

    public Task SendDataAsync(string deviceToken, IReadOnlyDictionary<string, string>? data)
    {
        throw new NotImplementedException();
    }

    public Task SendDataManyAsync(string[] deviceTokens, IReadOnlyDictionary<string, string>? data)
    {
        throw new NotImplementedException();
    }

    private async Task<Message> GetMessageAsync(string token, Guid notificationId,
        IReadOnlyDictionary<string, string>? data)
    {
        var notification = await _notificationService.GetNotificationAsync(notificationId);

        return GetMessage(token, notification.Title, notification.Content, data);
    }

    private async Task<List<Message>> GetMessageAsync(string[] tokens, Guid notificationId,
        IReadOnlyDictionary<string, string>? data)
    {
        var messages = new List<Message>();
        var notification = await _notificationService.GetNotificationAsync(notificationId);
        foreach (var token in tokens)
        {
            messages.Add(GetMessage(token, notification.Title, notification.Content, data));
        }

        return messages;
    }

    private Message GetMessage(string token, string title, string body,
        IReadOnlyDictionary<string, string>? data)
    {
        return new Message()
        {
            Token = token,
            Notification = new Notification
            {
                Title = title,
                Body = body
            },
            Data = data ?? new Dictionary<string, string>()
        };
    }

    private List<Message> GetMessage(string[] tokens, string title, string body,
        IReadOnlyDictionary<string, string>? data)
    {
        var messages = new List<Message>();
        foreach (var token in tokens)
        {
            messages.Add(GetMessage(token, title, body, data));
        }

        return messages;
    }

    private void ThrowIfSentFailed(string? result)
    {
        if (string.IsNullOrEmpty(result))
            throw new Exception($"Failed to send notification");
    }

    private void ThrowIfSentFailed(BatchResponse? result)
    {
        if (result == null)
            throw new Exception($"Failed to send notification");
        if (result.Responses.Any(x => !x.IsSuccess))
            throw new Exception($"Failed to send notification");
    }
}