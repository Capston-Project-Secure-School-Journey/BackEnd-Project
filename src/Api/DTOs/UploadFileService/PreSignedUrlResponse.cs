namespace Api.DTOs.UploadFileService;

public class PreSignedUrlResponse
{
    public string PreSignedUrl { get; set; } = string.Empty;
    public Guid FileKey { get; set; }
    public DateTime ExpiresAt { get; set; }
}