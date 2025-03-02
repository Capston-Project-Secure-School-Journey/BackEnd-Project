namespace Api.DTOs.UploadFileService;

public class UploadFileResponse
{
    public Guid Key { get; set; }
    public string ContentType { get; set; }
    public float Size { get; set; }
    public string S3Url { get; set; }
}