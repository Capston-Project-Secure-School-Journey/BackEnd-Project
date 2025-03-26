using System.ComponentModel.DataAnnotations;
using Api.Common.Enums;

namespace Api.DTOs.UploadFileService;

public class AddFileManagementDto
{
    [Required] public string FileName { get; set; } = null!;
    [Required] public string S3Key { get; set; } = null!;
    [Required] public string FileType { get; set; } = null!;
    [Required] public float FileSize { get; set; }
    public Guid? UploadBy { get; set; }
    public Guid? RelatedObjectId { get; set; }
    public RelatedObjectType? RelatedObjectType { get; set; }
}