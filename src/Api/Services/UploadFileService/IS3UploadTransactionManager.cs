namespace Api.Services.UploadFileService;

public interface IUploadTransactionManager
{
    Task TrackUploadedFile(string key);
    Task BeginTransactionAsync();
    Task RollbackAsync();
}