namespace Api.DTOs.UploadFileService;

public class PreSignedUrlResponse
{
    public string PreSignedUrl { get; set; } = string.Empty;
    public string FileKey { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}