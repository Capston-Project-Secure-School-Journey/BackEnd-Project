using Api.Common.Enums;
using Api.Security.CurrentUserProvider;
using Api.Services.ParentSchoolTripService;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs;

public interface ITypedHubClient
{
    [HubMethodName("send-driver-address")]
    Task SendDriverAddress(Guid tripId, double latitude, double longitude);
}

public class TripHub(
    ILogger<TripHub> logger,
    ICurrentUserProvider currentUserProvider,
    IParentSchoolTripService parentSchoolTripService) : Hub<ITypedHubClient>
{
    private static readonly ConnectionMapping<SocketIdentity> Connections = new();

    public override Task OnConnectedAsync()
    {
        var currentUser = currentUserProvider.GetCurrentUser();
        var key = new SocketIdentity(
            currentUser.UserId,
            currentUser.UserType
        );
        Connections.Add(key, Context.ConnectionId);

        logger.LogInformation($"New Connection: {key}");
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogInformation($"Disconnect: {exception?.Message}");
        try
        {
            var currentUser = currentUserProvider.GetCurrentUser();
            var key = new SocketIdentity(
                currentUser.UserId,
                currentUser.UserType
            );
            Connections.Remove(key, Context.ConnectionId);
            logger.LogInformation($"Disconnect: {key}");
        }
        catch (Exception)
        {
            Connections.Remove(Context.ConnectionId);
        }

        return base.OnDisconnectedAsync(exception);
    }

    [HubMethodName("subscribe-Trip-room")]
    public async Task SubscribeTripRoom(Guid tripId)
    {
        var currentUser = currentUserProvider.GetCurrentUser();
        if (currentUser.UserType == UserType.Parent &&
            await parentSchoolTripService.HasInProgressShuttle(currentUser.UserId, tripId))
            await Groups.AddToGroupAsync(Context.ConnectionId, tripId.ToString());
        else
            logger.LogInformation($"Can't subscribe trip room: {tripId}");
    }
}