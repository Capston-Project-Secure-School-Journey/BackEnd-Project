using Api.DTOs.UploadFileService;

namespace Api.Services.UploadFileService;

public interface IFileUploadService
{
    Task<UploadFileResponse> UploadFileAsync(IFormFile file, string prefix = "");

    Task<UploadFileResponse> UploadStreamAsync(Stream stream, string fileName, string contentType,
        string prefix = "");

    Task<bool> DeleteFileManagementAsync(Guid id);
    Task<bool> DeleteFileManagementAsync(List<Guid> ids);
    Task<PreSignedUrlResponse> GeneratePreSignedUploadUrlAsync(PreSignedUrlRequest request, int expirationMinutes = 60);
    Task<string> GeneratePreSignedDownloadUrlAsync(string key, int expirationMinutes = 60);
    Task<string> GeneratePreSignedDownloadUrlAsync(Guid fileManagementKey, int expirationMinutes = 60);
    Task<UploadFileResponse> CopyObjectAsync(Guid id, string prefix);
    Task RollBackAsync();
    Task BeginTransactionAsync();
}