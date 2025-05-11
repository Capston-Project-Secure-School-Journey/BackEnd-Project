using Api.Common.Exceptions;
using Api.Common.Utilities;
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
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(config["FcmSetting:CredentialsPath"]!)
            });

        _messaging = FirebaseMessaging.DefaultInstance;
    }

    public async Task SendAsync(string deviceToken, string title, string body)
    {
        ThrowIfDeviceTokensAreEmpty(deviceToken);
        await SendAsync(deviceToken, title, body, null);
    }

    public async Task SendAsync(string deviceToken, Guid notificationId)
    {
        ThrowIfDeviceTokensAreEmpty(deviceToken);
        var message = await GetMessageAsync(deviceToken, notificationId, null);
        var result = await _messaging.SendAsync(message);

        ThrowIfSentFailed(result);
    }

    public async Task SendAsync(string deviceToken, string title, string body,
        IReadOnlyDictionary<string, string>? data)
    {
        ThrowIfDeviceTokensAreEmpty(deviceToken);
        var message = GetMessage(deviceToken, title, body, data);
        var result = await _messaging.SendAsync(message);

        ThrowIfSentFailed(result);
    }

    public async Task SendAsync(string deviceToken, Guid notificationId, IReadOnlyDictionary<string, string>? data)
    {
        ThrowIfDeviceTokensAreEmpty(deviceToken);
        var message = await GetMessageAsync(deviceToken, notificationId, data);
        var result = await _messaging.SendAsync(message);

        ThrowIfSentFailed(result);
    }

    public async Task SendManyAsync(string[] deviceTokens, string title, string body)
    {
        ThrowIfDeviceTokensAreEmpty(deviceTokens);
        var messages = GetMessage(deviceTokens, title, body, null);
        var result = await _messaging.SendEachAsync(messages);

        ThrowIfSentFailed(result);
    }

    public async Task SendManyAsync(string[] deviceTokens, Guid notificationId)
    {
        ThrowIfDeviceTokensAreEmpty(deviceTokens);
        var messages = await GetMessageAsync(deviceTokens, notificationId, null);
        var result = await _messaging.SendEachAsync(messages);

        ThrowIfSentFailed(result);
    }

    public async Task SendManyAsync(string[] deviceTokens, string title, string body,
        IReadOnlyDictionary<string, string>? data)
    {
        ThrowIfDeviceTokensAreEmpty(deviceTokens);
        var messages = GetMessage(deviceTokens, title, body, data);
        var result = await _messaging.SendEachAsync(messages);

        ThrowIfSentFailed(result);
    }

    public async Task SendManyAsync(string[] deviceTokens, Guid notificationId,
        IReadOnlyDictionary<string, string>? data)
    {
        ThrowIfDeviceTokensAreEmpty(deviceTokens);
        var messages = await GetMessageAsync(deviceTokens, notificationId, data);
        var result = await _messaging.SendEachAsync(messages);

        ThrowIfSentFailed(result);
    }

    public Task SendDataAsync(string deviceToken, IReadOnlyDictionary<string, string>? data)
    {
        ThrowIfDeviceTokensAreEmpty(deviceToken);
        throw new NotImplementedException();
    }

    public Task SendDataManyAsync(string[] deviceTokens, IReadOnlyDictionary<string, string>? data)
    {
        ThrowIfDeviceTokensAreEmpty(deviceTokens);
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
        foreach (var token in tokens) messages.Add(GetMessage(token, notification.Title, notification.Content, data));

        return messages;
    }

    private static Message GetMessage(string token, string title, string body,
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

    private static List<Message> GetMessage(string[] tokens, string title, string body,
        IReadOnlyDictionary<string, string>? data)
    {
        var messages = new List<Message>();
        foreach (var token in tokens) messages.Add(GetMessage(token, title, body, data));

        return messages;
    }

    private static void ThrowIfSentFailed(string? result)
    {
        if (string.IsNullOrEmpty(result))
            throw new NotificationFailedException(ErrorMessages.NotificationSendFailure);
    }

    private static void ThrowIfSentFailed(BatchResponse? result)
    {
        if (result == null)
            throw new NotificationFailedException(ErrorMessages.NotificationSendFailure);
        if (result.Responses.Any(x => !x.IsSuccess))
            throw new NotificationFailedException(ErrorMessages.NotificationSendFailure);
    }

    private static void ThrowIfDeviceTokensAreEmpty(string[] tokens)
    {
        if (tokens == null || tokens.Length == 0)
            throw new NotificationFailedException(ErrorMessages.DeviceTokenCannotBeEmpty);
        foreach (var token in tokens)
        {
            if (string.IsNullOrEmpty(token))
                throw new NotificationFailedException(ErrorMessages.DeviceTokenCannotBeEmpty);
        }
    }

    private static void ThrowIfDeviceTokensAreEmpty(string token)
    {
        if (string.IsNullOrEmpty(token))
            throw new NotificationFailedException(ErrorMessages.DeviceTokenCannotBeEmpty);
    }
}