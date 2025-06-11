using System.Text;
using Api.Common.Utilities;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.NotificationService;
using Api.Extensions;
using Api.Services.NotificationService;
using Api.Services.UploadFileService;
using Api.Services.UserManagementService;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace Api.Jobs.ShuttleScheduleJobs;

public class AlertInsufficientDriversJob(
    IServiceProvider serviceProvider,
    ILogger<AlertInsufficientDriversJob> logger) : IJob
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2139",
        Justification = "Handled and logged in job context")]
    public async Task ExecuteAsync(params object[] args)
    {
        try
        {
            var schoolId = Guid.Parse(args[0].ToString()!);

            if (schoolId == Guid.Empty)
                throw new InvalidDataException(ErrorMessages.InvalidSchoolId);

            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<Context>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var userManagement = scope.ServiceProvider.GetRequiredService<IUserManagement>();
            var uploadFileService = scope.ServiceProvider.GetRequiredService<IFileUploadService>();

            var startDate = await db.SystemVariables
                .FirstOrDefaultAsync(e => e.Name == "START_DATE" && e.SchoolId == schoolId);

            if (startDate is null)
                return;

            var studentCount = await db.Students
                .Where(s => s.SchoolId == schoolId && s.NeedsPickup)
                .CountAsync();

            var seatingCapacityCount = await db.ActiveDrivers
                .AsNoTracking()
                .AsQueryable()
                .Where(d => d.SchoolId == schoolId &&
                            (d.ExpiredAt == null || d.ExpiredAt >= DateTimeHelper.GetDateTimeUtc7()))
                .SumAsync(d => d.SeatingCapacity);

            if (studentCount > seatingCapacityCount)
            {
                var drivers = await db.ActiveDrivers
                    .AsNoTracking()
                    .AsQueryable()
                    .Include(x => x.Driver)
                    .Where(d => d.SchoolId == schoolId &&
                                (d.ExpiredAt == null || d.ExpiredAt >= DateTimeHelper.GetDateTimeUtc7()))
                    .ToListAsync();
                var schoolAdmin = await userManagement.GetSchoolAdmin(schoolId);
                var csvFile = CreateCsvFile(drivers, studentCount);
                var trans = await db.Database.BeginTransactionAsync();
                await uploadFileService.BeginTransactionAsync();
                try
                {
                    var fileResponse = await uploadFileService.UploadStreamAsync(csvFile,
                        $"missing_address_{DateTimeHelper.GetDateTimeUtc7().ToShortDateString()}.csv",
                        "text/csv",
                        "batch_data");
                    var fileLink = await uploadFileService.GeneratePreSignedDownloadUrlAsync(fileResponse.Key, 2629800);

                    var dto = new CreateNotificationDto
                    {
                        Title = "Thiếu tài xế để vận hành hệ thống",
                        Content = $"Hệ thống hiện tại vẫn chưa đủ số lượng tài xế để vận hành đưa đón.<br>" +
                                  $"Vui lòng tuyển dụng thêm tài xế.<br>" +
                                  $"Chi tiết danh sách tài xế hiện có: <a href=\"{fileLink}\" target=\"_blank\">Tải xuống tại đây</a>.",
                        RecipientId = schoolAdmin.Id,
                        Navigation = string.Empty
                    };
                    var notification = await notificationService.CreateNotification(dto);
                    await trans.CommitAsync();

                    logger.LogInformation("Alert InsufficientDriversJob executed.");
                    BackgroundJob.Enqueue<SendNotificationJob>(
                        (job) => job.ExecuteAsync(new List<Guid> { notification.Id }));
                }
                catch (Exception)
                {
                    uploadFileService
                        .RollBackAsync()
                        .FireAndForget((ex) => logger.LogError(ex, "UploadFileService.RollBackAsync"));
                    throw;
                }
                finally
                {
                    await trans.DisposeAsync();
                    await csvFile.DisposeAsync();
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Alert InsufficientDriversJob failed.");
        }
    }

    private static MemoryStream CreateCsvFile(List<ActiveDriver> drivers, int studentCount)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream, Encoding.UTF8);

        writer.WriteLine("DriverId,FullName,SeatingCapacity,ExpiredAt");

        foreach (var d in drivers)
            writer.WriteLine(
                $"{d.DriverId},{EscapeCsv(d.Driver.FirstName + d.Driver.LastName)},{d.SeatingCapacity},{d.ExpiredAt}");

        writer.WriteLine($"Số lượng xe hiện tại:,{drivers.Count}");
        writer.WriteLine($"Số chỗ ngồi hiện tại:,{drivers.Sum(x => x.SeatingCapacity)}");
        writer.WriteLine($"Số lượng học sinh:,{studentCount}");

        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    private static string EscapeCsv(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        if (input.Contains(',') || input.Contains('"') || input.Contains('\n'))
            return $"\"{input.Replace("\"", "\"\"")}\"";

        return input;
    }
}