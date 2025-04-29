namespace Api.DTOs.UploadFileService;

public class UploadFileResponse
{
    public Guid Key { get; set; }
    public string S3Key { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public float Size { get; set; }
    public string S3Url { get; set; } = string.Empty;
}