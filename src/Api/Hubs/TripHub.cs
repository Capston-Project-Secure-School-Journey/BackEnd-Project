using Api.Attributes;
using Api.Common.Enums;
using Api.Domain.Models;
using Api.Extensions;
using Api.Jobs.SchoolTripEventDispatchJobs;
using Api.Security.CurrentUserProvider;
using Api.Services.DriverSchoolTripService;
using Api.Services.ParentSchoolTripService;
using Hangfire;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;

namespace Api.Hubs;

public interface ITypedHubClient
{
    [HubMethodName("send-driver-address")]
    Task SendDriverAddress(Guid tripId, double latitude, double longitude);
}

public class TripHub(
    ILogger<TripHub> logger,
    ICurrentUserProvider currentUserProvider,
    IParentSchoolTripService parentSchoolTripService,
    IDriverSchoolTripService driverSchoolTripService,
    IMemoryCache memoryCache) : Hub<ITypedHubClient>
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

        logger.LogInformation("New Connection: {key}", key);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogInformation("Disconnect: {Message}", exception?.Message);
        try
        {
            var currentUser = currentUserProvider.GetCurrentUser();
            var key = new SocketIdentity(
                currentUser.UserId,
                currentUser.UserType
            );
            Connections.Remove(key, Context.ConnectionId);
            logger.LogInformation("Disconnect: {key}", key);
        }
        catch (Exception)
        {
            Connections.Remove(Context.ConnectionId);
        }

        return base.OnDisconnectedAsync(exception);
    }

    [HubMethodName("subscribe-Trip-room")]
    [Authorize(UserType.Parent)]
    public async Task SubscribeTripRoom(Guid tripId)
    {
        var currentUser = currentUserProvider.GetCurrentUser();
        if (await parentSchoolTripService.HasInProgressShuttle(currentUser.UserId, tripId))
            await Groups.AddToGroupAsync(Context.ConnectionId, tripId.ToString());
        else
            logger.LogInformation("Can't subscribe trip room: {tripId}", tripId);
    }

    [HubMethodName("update-trip-location")]
    [Authorize(UserType.Driver)]
    public async Task UpdateTripLocation(double latitude, double longitude)
    {
        var currentUser = currentUserProvider.GetCurrentUser();
        logger.LogInformation("UpdateTripLocation called by {driverId}", currentUser.UserId);

        var key = $"TripHub_{currentUser.UserId}";
        var notificationKey = $"TripHub_{currentUser.UserId}_Notification";
        var errorKey = $"TripHub_{currentUser.UserId}_Error";

        if (memoryCache.TryGetValue(errorKey, out _))
        {
            logger.LogInformation("Driver {driverId} không có chuyến đi nào hiện tại.", currentUser.UserId);
            return;
        }

        if (!memoryCache.TryGetValue(key, out Guid tripId))
        {
            ShuttleSchedule trip;
            try
            {
                trip = await driverSchoolTripService.GetCurrentShuttleScheduleByDriver(currentUser.UserId);
            }
            catch (Exception)
            {
                memoryCache.Set(errorKey, true, TimeSpan.FromMinutes(5));
                return;
            }

            tripId = trip.Id;
            memoryCache.Set(key, trip.Id, TimeSpan.FromMinutes(5));

            driverSchoolTripService
                .UpdateCurrentAddress(tripId, currentUser.UserId, latitude, longitude)
                .FireAndForget((ex) => logger.LogError(ex, "DriverSchoolTripService.UpdateCurrentAddress"));
        }

        Clients
            .Group(tripId.ToString())
            .SendDriverAddress(tripId, latitude, longitude)
            .FireAndForget((ex) => logger.LogError(ex, "TripHub.SendDriverAddress"));

        if (!memoryCache.TryGetValue(notificationKey, out _))
        {
            BackgroundJob.Enqueue<SendDriverAddressNotificationJob>(
                (job) => job.ExecuteAsync(tripId));
            memoryCache.Set(notificationKey, true, TimeSpan.FromMinutes(2));
        }
    }
}