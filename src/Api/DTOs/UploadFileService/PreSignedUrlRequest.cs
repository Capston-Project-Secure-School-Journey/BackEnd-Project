namespace Api.DTOs.UploadFileService;

public class PreSignedUrlRequest
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string Prefix { get; set; }
}