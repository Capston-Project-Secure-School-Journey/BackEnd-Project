using Api.Common.Enums;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.NotificationService;
using Api.Scheduling;
using Api.Services.NotificationService;
using Api.Services.ShuttleScheduleManagementService;
using Hangfire;
using Microsoft.Extensions.Caching.Memory;

namespace Api.Jobs.SchoolTripEventDispatchJobs;

public class SendDriverAddressNotificationJob(
    IServiceProvider serviceProvider,
    ILogger<SendDriverAddressNotificationJob> logger,
    IMemoryCache cache) : IJob
{
    private const string CacheKey = "SendDriverAddressNotificationJob";
    private const int NotificationTriggerDistance = 200;

    public async Task ExecuteAsync(params object[] args)
    {
        try
        {
            if (args[0] is not ShuttleSchedule shuttleSchedule)
                throw new InvalidDataException("Invalid Shuttle Schedule");

            var scope = serviceProvider.CreateScope();

            var notifications = new List<CreateNotificationDto>();
            var notificationIds = new List<Guid>();
            foreach (var student in shuttleSchedule.Students
                         .Where(st => IStudentGroupingAlgorithm.Haversine(
                             shuttleSchedule.CurrentLat,
                             shuttleSchedule.CurrentLng,
                             st.PickupLat,
                             st.PickupLng) < NotificationTriggerDistance))
            {
                var cacheKey = $"{CacheKey}_{shuttleSchedule.Id}_{student.StudentId}";
                if (!cache.TryGetValue(cacheKey, out _))
                {
                    notifications.AddRange(GetNotificationDto(shuttleSchedule, student));
                    cache.Set(cacheKey, 1,
                        TimeSpan.FromHours(3));
                }
            }
            
            if (notifications.Count == 0)
                return;

            var db = scope.ServiceProvider.GetRequiredService<Context>();
            var notificationService =
                scope.ServiceProvider.GetRequiredService<INotificationService>();
            var trans = await db.Database.BeginTransactionAsync();
            try
            {
                foreach (var notificationDto in notifications)
                {
                    notificationIds.Add((await notificationService.CreateNotification(notificationDto)).Id);
                }

                await trans.CommitAsync();
            }
            finally
            {
                await trans.DisposeAsync();
            }

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
        var title = "Tài xế đang đến gần nhà.";
        var content = string.Empty;

        switch (shuttleSchedule.Type)
        {
            case ShuttleScheduleType.PickUp:
                content = "Hiện tại tài xế sắp đến gần nhà, học sinh hãy ra địa điểm đón để lên xe đúng giờ.";
                break;
            case ShuttleScheduleType.DropOff:
                content = "Hiện tại tài xế đã đến gần nhà, hãy ra chủ động ra đón học sinh.";
                break;
        }

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