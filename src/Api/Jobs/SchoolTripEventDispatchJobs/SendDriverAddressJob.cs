using System.Globalization;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.NotificationService;
using Api.Scheduling;
using Api.Services.NotificationService;
using Api.Services.ShuttleScheduleManagementService;
using Api.Services.UserService;
using Hangfire;

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
            var db = scope.ServiceProvider.GetRequiredService<Context>();
            var shuttleScheduleManagementService =
                scope.ServiceProvider.GetRequiredService<IShuttleScheduleManagementService>();
            var userService =
                scope.ServiceProvider.GetRequiredService<IUserService>();
            var notificationService =
                scope.ServiceProvider.GetRequiredService<INotificationService>();
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

            var task = notificationSender.SendDataManyAsync(deviceTokens, data, new TimeSpan(0, 0, 15));

            var notifications = new List<CreateNotificationDto>();
            var notificationIds = new List<Guid>();
            foreach (var student in shuttleSchedule.Students)
            {
                var distance = IStudentGroupingAlgorithm.Haversine(shuttleSchedule.CurrentLat,
                    shuttleSchedule.CurrentLng,
                    student.PickupLat,
                    student.PickupLng);

                if (distance < 200)
                {
                    notifications.AddRange(GetNotificationDto(shuttleSchedule, student));
                }
            }

            var trans = await db.Database.BeginTransactionAsync();
            try
            {
                foreach (var notificationDto in notifications)
                {
                    notificationIds.Add((await notificationService.CreateNotification(notificationDto)).Id);
                }
                await trans.CommitAsync();
            }
            catch (Exception)
            {
                await trans.RollbackAsync();
                throw;
            }

            await task;
            logger.LogInformation("Sending driver address job successfully");
            BackgroundJob.Enqueue<SendNotificationJob>(
                (job) => job.ExecuteAsync(notificationIds));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while executing SendDriverAddressJob");
        }
    }

    private static List<CreateNotificationDto> GetNotificationDto(ShuttleSchedule shuttleSchedule,
        StudentOnBus studentOnBus)
    {
        var title = "Tài xế đã đang đến gần nhà.";
        var content = "Hiện tại tài xế sắp đến gần, hãy đưa con để lên xe.";
        var recipientIds = studentOnBus.Parents.Select(x => x.ParentId);
        return recipientIds.Select(x => new CreateNotificationDto()
        {
            Title = title,
            Content = content,
            RecipientId = x,
            Navigation = string.Empty
        }).ToList();
    }
}