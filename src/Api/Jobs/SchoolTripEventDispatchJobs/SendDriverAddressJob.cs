using System.Globalization;
using Api.Services.NotificationService;
using Api.Services.ShuttleScheduleManagementService;
using Api.Services.UserService;

namespace Api.Jobs.SchoolTripEventDispatchJobs;

public class SendDriverAddressJob(
    IServiceProvider serviceProvider,
    ILogger<SendDriverAddressJob> logger) : IJob
{
    public async Task ExecuteAsync(params object[] args)
    {
        try
        {
            var shuttleScheduleId = Guid.Parse(args[0].ToString()!);

            if (shuttleScheduleId == Guid.Empty)
                throw new InvalidDataException("Invalid Shuttle Schedule Id");
            
            var scope = serviceProvider.CreateScope();
            var shuttleScheduleManagementService =
                scope.ServiceProvider.GetRequiredService<IShuttleScheduleManagementService>();
            var userService =
                scope.ServiceProvider.GetRequiredService<IUserService>();
            var notificationSender =
                scope.ServiceProvider.GetRequiredService<INotificationSender>();
            
            var shuttleSchedule = await shuttleScheduleManagementService.GetShuttleSchedule(shuttleScheduleId);
            var parentIds = shuttleSchedule
                .Students
                .SelectMany(st => st.Parents)
                .Select(p => p.ParentId)
                .ToList();
            string[] deviceTokens = [];
            
            foreach (var parentId in parentIds)
            {
                deviceTokens = deviceTokens.Concat(await userService.GetDeviceTokens(parentId)).Distinct().ToArray();
            }

            var data = new Dictionary<string, string>();
            data.Add("ShuttleScheduleId", shuttleScheduleId.ToString());
            data.Add("Lat", shuttleSchedule.CurrentLat.ToString(CultureInfo.InvariantCulture));
            data.Add("Lng", shuttleSchedule.CurrentLng.ToString(CultureInfo.InvariantCulture));
            
            await notificationSender.SendDataManyAsync(deviceTokens, data, new TimeSpan(0, 0, 15));
            
            logger.LogInformation("Sending driver address job successfully");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while executing SendDriverAddressJob");
        }
    }
}