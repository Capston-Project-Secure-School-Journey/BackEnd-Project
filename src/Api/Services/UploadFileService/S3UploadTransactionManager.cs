using Api.Common.Utilities;

namespace Api.Services.UploadFileService;

public class UploadTransactionManager(IFileDeleter fileDeleter, ILogger<UploadTransactionManager> logger)
    : IUploadTransactionManager
{
    private readonly List<string> _uploadedKeys = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task TrackUploadedFile(string key)
    {
        await _semaphore.WaitAsync();
        try
        {
            if (!string.IsNullOrWhiteSpace(key))
                _uploadedKeys.Add(key);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task BeginTransactionAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            _uploadedKeys.Clear();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task RollbackAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            var tasks = new List<Task>();
            if (_uploadedKeys.Count <= 0)
                return;

            tasks.AddRange(_uploadedKeys.Select(key => Task.Run(async () =>
            {
                try
                {
                    await fileDeleter.DeleteFileAsync(key);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, ErrorMessages.FileDeleteError);
                }
            })));
            await Task.WhenAll(tasks);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}