using System.Text;
using Api.Common.Enums;
using Api.Common.Utilities;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.NotificationService;
using Api.Extensions;
using Api.Services.NotificationService;
using Api.Services.UploadFileService;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace Api.Jobs;

public class AlertMissingAddressJob(IServiceProvider serviceProvider) : IJob
{
    public async Task ExecuteAsync( params object[] args)
    {
        var schoolId = Guid.Parse(args[0].ToString()!);

        if (schoolId == Guid.Empty)
            throw new InvalidDataException(ErrorMessages.InvalidSchoolId);

        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Context>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var uploadFileService = scope.ServiceProvider.GetRequiredService<IFileUploadService>();

        var startDate = await db.SystemVariables
            .FirstOrDefaultAsync(e => e.Name == "START_DATE" && e.SchoolId == schoolId);

        if (startDate is null)
            return;
        if (Convert.ToDateTime(startDate.Value) <= DateTimeHelper.GetDateTimeUtc7())
            return;

        var studentNeedAlert = await db.Students
            .AsNoTracking()
            .AsQueryable()
            .Include(s => s.Class)
            .Where(s => s.SchoolId == schoolId)
            .Where(s => s.NeedsPickup)
            .Where(s => string.IsNullOrEmpty(s.PickUpLocation) || s.PickUpLat == 0 || s.PickUpLng == 0)
            .ToListAsync();

        if (studentNeedAlert.Count == 0)
            return;

        var admin = await db.SchoolPersons.AsNoTracking()
            .FirstOrDefaultAsync(ad => ad.SchoolId == schoolId && ad.UserType == UserType.SchoolAdmin);

        if (admin == null)
            throw new InvalidDataException(ErrorMessages.InvalidSchoolId);

        var trans = await db.Database.BeginTransactionAsync();
        var notificationIds = new List<Guid>();
        try
        {
            var csvFile = CreateCsvFile(studentNeedAlert);
            var fileResponse = await uploadFileService.UploadStreamAsync(csvFile,
                $"missing_address_{DateTimeHelper.GetDateTimeUtc7().ToShortDateString()}.csv",
                "text/csv",
                "batch_data");
            var fileLink = await uploadFileService.GeneratePreSignedDownloadUrlAsync(fileResponse.Key, 2629800);

            foreach (var student in studentNeedAlert)
            {
                var parentIds = student.ManagedBy.Select(x => x.ParentId).ToList();
                foreach (var parentId in parentIds)
                {
                    var dto = new CreateNotificationDto()
                    {
                        Title = $"Địa chỉ đón học sinh: {student.FullName} chưa được cập nhập",
                        Content =
                            "Hiện tại địa chỉ đón con bạn vẫn chưa được cập nhập.\nVui lòng cập nhập trong thời gian sớm nhất!",
                        RecipientId = parentId,
                        Navigation = string.Empty,
                    };
                    notificationIds.Add((await notificationService.CreateNotification(dto)).Id);
                }
            }

            var d = new CreateNotificationDto
            {
                Title = "Vẫn còn học sinh chưa được cập nhập địa chỉ",
                Content = $"Hiện tại địa chỉ đón đưa của một số học sinh vẫn chưa được cập nhập.<br>" +
                          $"Hãy yêu cầu học sinh cập nhập trước ngày bắt đầu chạy.<br>" +
                          $"Danh sách chi tiết: <a href=\"{fileLink}\" target=\"_blank\">Link</a>",
                RecipientId = admin.Id,
                Navigation = string.Empty,
            };

            notificationIds.Add((await notificationService.CreateNotification(d)).Id);
            await trans.CommitAsync();
        }
        catch (Exception)
        {
            await trans.RollbackAsync();
            _ = uploadFileService.RollBackAsync();
            throw;
        }

        BackgroundJob.Enqueue<SendNotificationJob>((job) => job.ExecuteAsync(notificationIds));
    }

    private static MemoryStream CreateCsvFile(List<Student> students)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream, Encoding.UTF8);

        writer.WriteLine("Id,FullName,ClassId,ClassName");

        foreach (var s in students)
        {
            writer.WriteLine($"{s.Id},{EscapeCsv(s.FullName)},{s.ClassId},{EscapeCsv(s.Class.ClassName)}");
        }

        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    private static string EscapeCsv(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        if (input.Contains(',') || input.Contains('"') || input.Contains('\n'))
        {
            return $"\"{input.Replace("\"", "\"\"")}\"";
        }

        return input;
    }
}