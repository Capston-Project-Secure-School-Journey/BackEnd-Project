using Api.Common.Enums;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.NotificationService;
using Api.Services.NotificationService;
using Api.Services.ShuttleScheduleManagementService;
using Hangfire;
using Newtonsoft.Json;

namespace Api.Jobs.SchoolTripEventDispatchJobs;

public class SendStudentTripEventJob(
    IServiceProvider serviceProvider,
    ILogger<SendStudentTripEventJob> logger) : IJob
{
    public async Task ExecuteAsync(params object[] args)
    {
        try
        {
            var shuttleScheduleId = Guid.Parse(args[0].ToString()!);
            var studentTripEvent = Enum.Parse<StudentTripEvent>(args[1].ToString()!);
            var studentId = Guid.Parse(args[2].ToString()!);

            if (shuttleScheduleId == Guid.Empty)
                throw new InvalidDataException("Invalid Shuttle Schedule Id");
            if (studentId == Guid.Empty)
                throw new InvalidDataException("Invalid Student Id");

            var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<Context>();
            var shuttleScheduleManagementService =
                scope.ServiceProvider.GetRequiredService<IShuttleScheduleManagementService>();
            var notificationService =
                scope.ServiceProvider.GetRequiredService<INotificationService>();

            var shuttleSchedule = await shuttleScheduleManagementService.GetShuttleSchedule(shuttleScheduleId);
            var notificationIds = new List<Guid>();
            var studentOnBus = shuttleSchedule.Students.First(st => st.StudentId == studentId);

            var notificationSendToParent =
                GetNotificationSendToParentDto(studentTripEvent, studentOnBus);
            var notificationSendToDriver =
                GetNotificationSendToDriverDto(shuttleSchedule, studentTripEvent, studentOnBus);
            var notificationSendToDriverId = Guid.Empty;

            var trans = await db.Database.BeginTransactionAsync();
            try
            {
                foreach (var notification in notificationSendToParent)
                {
                    notificationIds.Add((await notificationService.CreateNotification(notification)).Id);
                }

                if (notificationSendToDriver != null)
                    notificationSendToDriverId =
                        (await notificationService.CreateNotification(notificationSendToDriver)).Id;

                await trans.CommitAsync();
            }
            catch (Exception)
            {
                await trans.RollbackAsync();
                throw;
            }

            logger.LogInformation("SendStudentTripEventJob successfully");
            if (notificationSendToDriverId != Guid.Empty)
            {
                var data = new Dictionary<string, string>
                {
                    { "shuttleScheduleId", shuttleScheduleId.ToString() },
                    { "StudentInfo", JsonConvert.SerializeObject(studentOnBus) }
                };
                BackgroundJob.Enqueue<SendNotificationJob>(
                    (job) => job.ExecuteAsync(new List<Guid>() { notificationSendToDriverId }, data));
            }
            
            BackgroundJob.Enqueue<SendNotificationJob>(
                (job) => job.ExecuteAsync(notificationIds));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while executing SendStudentTripEventJob");
        }
    }

    private static List<CreateNotificationDto> GetNotificationSendToParentDto(StudentTripEvent eventType, StudentOnBus studentOnBus)
    {
        var title = "";
        var content = "";
        var recipientIds = studentOnBus.Parents.Select(x => x.ParentId);
        switch (eventType)
        {
            case StudentTripEvent.PickedUp:
                title += $"Con của bạn lên xe";
                content += $"Con của bạn lên xe lúc: {studentOnBus.PickedUpTime}";
                break;
            case StudentTripEvent.DroppedOff:
                title += $"Con của bạn xuống xe";
                content += $"Con của bạn xuống xe lúc: {studentOnBus.DroppedOffTime}";
                break;
            case StudentTripEvent.SkippedFromDriver:
                title += "Con của bạn được xác nhận không cần đón.";
                content +=
                    $"Con của bạn được xác nhận không cần đón từ tài xế. Lí do: {studentOnBus.IsSkipUpReason}\n" +
                    $"Nếu có bất kì nhầm lẫn nào hãy liên hệ tài xế.";
                break;
            default:
                return new List<CreateNotificationDto>();
        }

        return recipientIds.Select(x => new CreateNotificationDto()
        {
            Title = title,
            Content = content,
            RecipientId = x,
            Navigation = string.Empty
        }).ToList();
    }

    private static CreateNotificationDto? GetNotificationSendToDriverDto(ShuttleSchedule shuttleSchedule,
        StudentTripEvent eventType, StudentOnBus studentOnBus)
    {
        var title = "";
        var content = "";
        switch (eventType)
        {
            case StudentTripEvent.PickedUp:
                title += $"Học sinh {studentOnBus.FullName} vừa lên xe.";
                content += $"Học sinh {studentOnBus.FullName} vừa lên xe. Hãy xác nhận lại thông qua ảnh và thông tin.";
                break;
            case StudentTripEvent.DroppedOff:
                title += $"Học sinh {studentOnBus.FullName} vừa xuống xe.";
                content +=
                    $"Học sinh {studentOnBus.FullName} vừa xuống xe. Hãy xác nhận lại thông qua ảnh và thông tin.";
                break;
            case StudentTripEvent.SkippedFromParent:
                title += "Có học sinh đã được yêu cầu không cần đón từ phụ huynh.";
                content += $"Học sinh {studentOnBus.FullName} đã được yêu cầu không cần đón từ phụ huynh.\n" +
                           $"Xác nhận thông tin và bỏ qua việc đưa đón học sinh này.";
                break;
            default:
                return null;
        }

        return new CreateNotificationDto()
        {
            Title = title,
            Content = content,
            RecipientId = shuttleSchedule.DriverId,
            Navigation = string.Empty
        };
    }
}