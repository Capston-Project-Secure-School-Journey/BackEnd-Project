using Api.Common.Enums;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.NotificationService;
using Api.Services.NotificationService;
using Api.Services.ShuttleScheduleManagementService;
using Hangfire;

namespace Api.Jobs.SchoolTripEventDispatchJobs;

public class SendSchoolTripEventJob(
    IServiceProvider serviceProvider,
    ILogger<SendSchoolTripEventJob> logger) : IJob
{
    public async Task ExecuteAsync(params object[] args)
    {
        try
        {
            var shuttleScheduleId = Guid.Parse(args[0].ToString()!);
            var schoolTripEvent = Enum.Parse<SchoolTripEvent>(args[1].ToString()!);

            if (shuttleScheduleId == Guid.Empty)
                throw new InvalidDataException("Invalid Shuttle Schedule Id");

            var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<Context>();
            var shuttleScheduleManagementService =
                scope.ServiceProvider.GetRequiredService<IShuttleScheduleManagementService>();
            var notificationService =
                scope.ServiceProvider.GetRequiredService<INotificationService>();

            var shuttleSchedule = await shuttleScheduleManagementService.GetShuttleSchedule(shuttleScheduleId);
            var notificationIds = new List<Guid>();
            var trans = await db.Database.BeginTransactionAsync();
            try
            {
                foreach (var student in shuttleSchedule.Students)
                {
                    var parentIds = student.Parents
                        .Select(p => p.ParentId)
                        .ToList();

                    foreach (var parentId in parentIds)
                    {
                        var notification = await notificationService.CreateNotification(GetNotificationDto(
                            shuttleSchedule,
                            schoolTripEvent,
                            parentId,
                            student));
                        notificationIds.Add(notification.Id);
                    }
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

    private static CreateNotificationDto GetNotificationDto(ShuttleSchedule shuttleSchedule, SchoolTripEvent eventType,
        Guid parentId,
        StudentOnBus studentOnBus)
    {
        var title = "";
        var content = "";
        var shuttleScheduleType = shuttleSchedule.Type == ShuttleScheduleType.PickUp ? "đón học sinh" : "trả học sinh";
        switch (eventType)
        {
            case SchoolTripEvent.CommandStartedEvent:
                title += "Chuyến " + shuttleScheduleType + " đã bắt đầu.";
                content += "Hãy chú ý thời gian và vị trí của tài xế để đưa đón con được thuận lợi.";
                break;
            case SchoolTripEvent.CommandCompletedEvent:
                title += "Chuyến " + shuttleScheduleType + " đã hoàn thành.";
                content += $"Con bạn đã được đón vào lúc: {studentOnBus.PickedUpTime?.DateTime.ToShortDateString()}.\n"
                           + $"Con bạn đã được trả vào lúc: {studentOnBus.DroppedOffTime?.DateTime.ToShortDateString()}.";
                break;
            case SchoolTripEvent.CommandCancelledEvent:
                title += "Chuyến " + shuttleScheduleType + " đã bị hủy.";
                content += $"Lí do hủy: {shuttleSchedule.CancelReason}";
                break;
        }

        return new CreateNotificationDto()
        {
            Title = title,
            Content = content,
            RecipientId = parentId,
            Navigation = string.Empty
        };
    }
}