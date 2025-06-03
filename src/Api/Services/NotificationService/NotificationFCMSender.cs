using Api.Common.Exceptions;
using Api.Common.Utilities;
using Api.Services.UserService;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace Api.Services.NotificationService;

public class NotificationFcmSender : INotificationSender
{
    private readonly INotificationService _notificationService;
    private readonly FirebaseMessaging _messaging;
    private readonly IUserService _userService;
    private static readonly object FirebaseAppLock = new();

    public NotificationFcmSender(IConfiguration config, INotificationService notificationService,
        IUserService userService)
    {
        _notificationService = notificationService;
        lock (FirebaseAppLock)
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                var options = new AppOptions
                {
                    Credential = GoogleCredential.FromFile(config["FcmSetting:CredentialsPath"]!)
                };
                FirebaseApp.Create(options);
            }
        }

        _messaging = FirebaseMessaging.DefaultInstance;
        _userService = userService;
    }

    public async Task SendOneAsync(string deviceToken, string title, string body,
        IReadOnlyDictionary<string, string>? data)
    {
        ThrowIfDeviceTokensAreEmpty(deviceToken);
        var message = GetMessage(deviceToken, title, body, data);
        var result = await _messaging.SendAsync(message);

        ThrowIfSentFailed(result);
    }

    public async Task SendByNotificationAsync(Guid notificationId, IReadOnlyDictionary<string, string>? data)
    {
        var messages = await GetMessageAsync(notificationId, data);
        if (messages.Count == 0) return;

        var result = await _messaging.SendEachAsync(messages);

        ThrowIfSentFailed(result);
    }

    public async Task SendByNotificationManyAsync(List<Guid> notificationIds,
        IReadOnlyDictionary<string, string>? data)
    {
        var messages = new List<Message>();
        foreach (var notificationId in notificationIds)
        {
            messages.AddRange(await GetMessageAsync(notificationId, data));
        }

        if (messages.Count == 0) return;
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

    public async Task SendDataAsync(string deviceToken, IReadOnlyDictionary<string, string> data)
    {
        ThrowIfDeviceTokensAreEmpty(deviceToken);
        var message = GetDataMessage(deviceToken, data);
        var result = await _messaging.SendAsync(message);

        ThrowIfSentFailed(result);
    }

    public async Task SendDataManyAsync(string[] deviceTokens, IReadOnlyDictionary<string, string> data)
    {
        ThrowIfDeviceTokensAreEmpty(deviceTokens);
        var message = GetDataMessage(deviceTokens, data);
        var result = await _messaging.SendEachAsync(message);

        ThrowIfSentFailed(result);
    }

    public async Task SendDataToTopicAsync(string topic, IReadOnlyDictionary<string, string> data)
    {
        var message = GetDataMessageTopic(topic, data);
        var result = await _messaging.SendAsync(message);
        ThrowIfSentFailed(result);
    }

    private async Task<List<Message>> GetMessageAsync(Guid notificationId,
        IReadOnlyDictionary<string, string>? data)
    {
        var messages = new List<Message>();
        var notification = await _notificationService.GetNotificationAsync(notificationId);
        var deviceTokens = await _userService.GetDeviceTokens(notification.RecipientId);
        foreach (var token in deviceTokens)
            messages.Add(GetMessage(token, notification.Title, notification.Content, data));

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

    private static Message GetDataMessage(string token
        , IReadOnlyDictionary<string, string> data)
    {
        return new Message()
        {
            Notification = new Notification
            {
                Title = "SSAST_Data",
                Body = ""
            },
            Token = token,
            Data = data
        };
    }

    private static Message GetDataMessageTopic(string topic
        , IReadOnlyDictionary<string, string> data)
    {
        return new Message()
        {
            Notification = new Notification
            {
                Title = "SSAST_Data",
                Body = ""
            },
            Data = data,
            Topic = topic,
        };
    }

    private static List<Message> GetDataMessage(string[] tokens
        , IReadOnlyDictionary<string, string> data)
    {
        var messages = new List<Message>();
        foreach (var token in tokens) messages.Add(GetDataMessage(token, data));
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
        if (tokens.Any(string.IsNullOrEmpty))
            throw new NotificationFailedException(ErrorMessages.DeviceTokenCannotBeEmpty);
    }

    private static void ThrowIfDeviceTokensAreEmpty(string token)
    {
        if (string.IsNullOrEmpty(token))
            throw new NotificationFailedException(ErrorMessages.DeviceTokenCannotBeEmpty);
    }
}