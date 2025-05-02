namespace Api.Services.UploadFileService;

public interface IFileDeleter
{
    Task<bool> DeleteFileAsync(string key);
}