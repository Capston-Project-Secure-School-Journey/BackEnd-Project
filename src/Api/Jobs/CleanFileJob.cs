using Api.Domain;
using Api.Extensions;
using Api.Services.UploadFileService;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Api.Jobs;

public class CleanFileJob(
    Context context,
    IFileDeleter fileDeleter,
    ILogger<CleanFileJob> logger) : IJob
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2139",
        Justification = "Handled and logged in job context")]
    public async Task ExecuteAsync(params object[] args)
    {
        IDbContextTransaction? trans = null;
        try
        {
            context.BypassSoftDelete = true;
            var filesDeleted = context
                .FileManagements
                .IgnoreQueryFilters()
                .Where(f => f.IsDeleted || !f.IsUploaded)
                .AsEnumerable()
                .Where(f => f.IsDeleted ||
                            (!f.IsUploaded && (DateTimeHelper.GetDateTimeUtc7() - f.CreatedAt).Days > 3)
                )
                .ToList();

            if (filesDeleted.Count == 0)
            {
                logger.LogInformation("Clean files successfully");
                logger.LogInformation("No files deleted");
                return;
            }

            var tasks = new List<Task>();

            trans = await context.Database.BeginTransactionAsync();
            context.FileManagements.RemoveRange(filesDeleted);
            tasks.Add(context.SaveChangesAsync());

            foreach (var s3Key in filesDeleted.Select(f => f.S3Key)) tasks.Add(fileDeleter.DeleteFileAsync(s3Key));

            await Task.WhenAll(tasks);
            await trans.CommitAsync();
            logger.LogInformation("Clean files successfully");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Clean files failed");
            if (trans != null)
                await trans.DisposeAsync();
        }
    }
}