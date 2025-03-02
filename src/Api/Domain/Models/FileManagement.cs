using Api.Common.Enums;

namespace Api.Domain.Models;

public class FileManagement: BaseModel
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string S3Key { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public float FileSize { get; set; }
    public DateTime UploadDate { get; set; }
    public Guid? UploadBy { get; set; }
    public Guid? RelatedObjectId { get; set; }
    public RelatedObjectType? RelatedObjectType { get; set; }
    public bool IsUploaded { get; set; }
}