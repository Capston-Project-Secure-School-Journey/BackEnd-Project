namespace Api.Domain.Models;

public class FileMetadata
{
    public Guid FileManagementId { get; set; }
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string Key { get; set; } = string.Empty;
}