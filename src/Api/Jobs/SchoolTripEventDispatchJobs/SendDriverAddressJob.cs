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
            var shuttleScheduleId = Guid.Parse(args[0].ToString()!);

            if (shuttleScheduleId == Guid.Empty)
                throw new InvalidDataException("Invalid Shuttle Schedule Id");

            var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<Context>();
            var shuttleScheduleManagementService =
                scope.ServiceProvider.GetRequiredService<IShuttleScheduleManagementService>();
            var notificationService =
                scope.ServiceProvider.GetRequiredService<INotificationService>();

            var shuttleSchedule = await shuttleScheduleManagementService.GetShuttleSchedule(shuttleScheduleId);

            var notifications = new List<CreateNotificationDto>();
            var notificationIds = new List<Guid>();
            foreach (var student in shuttleSchedule.Students
                         .Where(st => IStudentGroupingAlgorithm.Haversine(
                             shuttleSchedule.CurrentLat,
                             shuttleSchedule.CurrentLng,
                             st.PickupLat,
                             st.PickupLng) < NotificationTriggerDistance))
            {
                var cacheKey = $"{CacheKey}_{shuttleScheduleId}_{student.StudentId}";
                if (!cache.TryGetValue(cacheKey, out _))
                {
                    notifications.AddRange(GetNotificationDto(shuttleSchedule, student));
                    cache.Set(cacheKey, 1,
                        TimeSpan.FromHours(1));
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
                content = "Hiện tại tài xế sắp đến gần nhà, hãy đưa con ra để lên xe đúng giờ.";
                break;
            case ShuttleScheduleType.DropOff:
                content = "Hiện tại tài xế đã đến gần nhà, hãy ra đón con.";
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