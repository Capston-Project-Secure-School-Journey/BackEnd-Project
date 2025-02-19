using Api.Common.Enums;

namespace Api.Domain.Models;

public class FileManagement: BaseModel
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string S3Url { get; set; } = string.Empty;
    public FileType FileType { get; set; }
    public float FileSize { get; set; }
    public DateTime UploadDate { get; set; }
    public Guid UploadBy { get; set; }
    public Guid RelatedObjectId { get; set; }
    public RelatedObjectType RelatedObjectType { get; set; }
}