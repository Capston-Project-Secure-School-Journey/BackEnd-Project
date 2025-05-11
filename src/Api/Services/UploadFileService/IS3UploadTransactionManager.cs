namespace Api.Services.UploadFileService;

public interface IUploadTransactionManager
{
    void TrackUploadedFile(string key);
    Task BeginTransactionAsync();
    Task RollbackAsync();
}