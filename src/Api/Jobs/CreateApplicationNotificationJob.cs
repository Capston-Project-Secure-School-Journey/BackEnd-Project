using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.NotificationService;
using Api.Services.ApplicationService;
using Api.Services.ApprovalProcessor;
using Api.Services.NotificationService;
using Hangfire;

namespace Api.Jobs;

public class CreateApplicationNotificationJob(IServiceProvider serviceProvider) : IJob
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2139",
        Justification = "Handled and logged in job context")]
    public async Task ExecuteAsync(params object[] args)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<CreateApplicationNotificationJob>>();
        try
        {
            var db = scope.ServiceProvider.GetRequiredService<Context>();
            var applicationService = scope.ServiceProvider.GetRequiredService<IApplicationService>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var approvalProcessor = scope.ServiceProvider.GetRequiredService<IApprovalProcessor>();

            if (!Guid.TryParse(args[0] as string, out var applicationId))
                return;

            var application = await applicationService
                .GetApplication(applicationId);
            var entity = db.Entry(application);
            if (!entity.Collection<DriverRequestStatusHistory>(x => x.DriverRequestStatusHistories).IsLoaded)
                await entity.Collection<DriverRequestStatusHistory>(x => x.DriverRequestStatusHistories).LoadAsync();

            var lastStatus = application
                .DriverRequestStatusHistories
                .OrderByDescending(x => x.ChangedAt)
                .FirstOrDefault();
            if (lastStatus == null)
                return;

            var recipientId = lastStatus.ChangedBy == application.DriverId
                ? await approvalProcessor.GetReviewerOfSchool(application.SchoolId)
                : application.DriverId;
            var createNotificationDto = new CreateNotificationDto()
            {
                Title = "Có thay đổi mới về đơn yêu cầu chấp nhận từ nhà trường",
                Content = ApplicationService.GetApplicationNotificationMessage(application),
                RecipientId = recipientId,
                Navigation = string.Empty
            };

            var notification = await notificationService.CreateNotification(createNotificationDto);
            
            logger.LogInformation("Application notification created successfully");
            BackgroundJob.Enqueue<SendNotificationJob>(
                (job) => job.ExecuteAsync(new List<Guid> { notification.Id }));
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occured while creating application notification");
        }
    }
}