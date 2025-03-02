using Api.DTOs.UploadFileService;

namespace Api.Services.UploadFileService;

public interface IFileUploadService
{
    Task<UploadFileResponse> UploadFileAsync(IFormFile file, string prefix = "");

    Task<UploadFileResponse> UploadStreamAsync(Stream stream, string fileName, string contentType,
        string prefix = "");

    Task<bool> DeleteFileAsync(string key, Guid? id);
    Task<PreSignedUrlResponse> GeneratePreSignedUploadUrlAsync(PreSignedUrlRequest request, int expirationMinutes = 60);
    Task<string> GeneratePreSignedDownloadUrlAsync(string key, int expirationMinutes = 60);
}