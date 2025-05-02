using Api.Common.Utilities;

namespace Api.Services.UploadFileService;

public class UploadTransactionManager(IFileDeleter fileDeleter, ILogger<UploadTransactionManager> logger)
    : IUploadTransactionManager
{
    private readonly List<string> _uploadedKeys = new();
    private readonly object _lock = new();

    public void TrackUploadedFile(string key)
    {
        if (!string.IsNullOrWhiteSpace(key))
        {
            lock (_lock)
            {
                _uploadedKeys.Add(key);
            }
        }
    }

    public async Task RollbackAsync()
    {
        var tasks = new List<Task>();
        lock (_lock)
        {
            if (_uploadedKeys.Count <= 0)
                return;

            foreach (var key in _uploadedKeys)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await fileDeleter.DeleteFileAsync(key);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, ErrorMessages.FileDeleteError);
                    }
                }));
            }
        }

        await Task.WhenAll(tasks);
    }
}