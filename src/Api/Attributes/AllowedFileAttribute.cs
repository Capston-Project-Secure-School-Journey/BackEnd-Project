using System.ComponentModel.DataAnnotations;
using Api.Common.Enums;
using Api.Extensions;

namespace Api.Attributes;

public class AllowedFileAttribute : ValidationAttribute
{
    private readonly ContentTypeEnum[] _allowedContentTypes;
    private readonly long _maxFileSizeInBytes;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllowedFileAttribute"/> class.
    /// </summary>
    /// <param name="allowedContentTypes">Allowed content types (MIME types)</param>
    /// <param name="maxFileSizeInMb">Maximum file size in megabytes</param>
    public AllowedFileAttribute(ContentTypeEnum[] allowedContentTypes, int maxFileSizeInMb = 1)
    {
        _allowedContentTypes = allowedContentTypes ?? throw new ArgumentNullException(nameof(allowedContentTypes));
        _maxFileSizeInBytes = maxFileSizeInMb * 1024 * 1024;
    }

    /// <summary>
    /// Validates the specified IFormFile.
    /// </summary>
    /// <param name="value">The file to validate.</param>
    /// <param name="validationContext">The validation context.</param>
    /// <returns>A <see cref="ValidationResult"/> object.</returns>
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        // Handle null file
        if (value == null)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return ValidationResult.Success;
        }

        if (value is not IFormFile file) return new ValidationResult("Invalid file type. Expected IFormFile.");

        if (file.Length == 0) return new ValidationResult("File không hợp lệ.");

        if (file.Length > _maxFileSizeInBytes)
            return new ValidationResult($"File không được quá {_maxFileSizeInBytes / (1024 * 1024)} MB.");

        var contentType = file.ContentType.ToLowerInvariant();
        if (_allowedContentTypes.All(t => t.GetDescription() != contentType))
            return new ValidationResult(
                $"Loại file '{contentType}' không được cho phép tải. Chỉ chấp nhận: {string.Join(", ", _allowedContentTypes)}");

        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExtensions = _allowedContentTypes
            .Select(x => GetExtensionFromContentType(x.GetDescription()))
            .Where(ext => !string.IsNullOrEmpty(ext))
            .ToArray();

        if (allowedExtensions.Length > 0 && !allowedExtensions.Contains(fileExtension))
            return new ValidationResult(
                $"Loại file '{fileExtension}' không được phép tải. Chỉ chấp nhận: {string.Join(", ", _allowedContentTypes)}");

        return ValidationResult.Success;
#pragma warning restore CS8603 // Possible null reference return.
    }

    /// <summary>
    /// Converts common content types to file extensions.
    /// </summary>
    /// <param name="contentType">MIME content type.</param>
    /// <returns>File extension or empty string.</returns>
    private string GetExtensionFromContentType(string contentType)
    {
        return contentType.ToLowerInvariant() switch
        {
            "image/jpg" => ".jpg",
            "image/jpeg" => ".jpeg",
            "image/png" => ".png",
            "image/gif" => ".gif",
            "image/bmp" => ".bmp",
            "image/tiff" => ".tiff",
            "image/webp" => ".webp",
            "image/svg+xml" => ".svg",
            
            "image/heic" => ".heic",
            "image/heics" => ".heics",
            "image/heif" => ".heif",
            
            "application/msword" => ".doc",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => ".docx",

            "application/pdf" => ".pdf",

            "application/vnd.ms-excel" => ".xls",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => ".xlsx",

            "application/vnd.ms-powerpoint" => ".ppt",
            "application/vnd.openxmlformats-officedocument.presentationml.presentation" => ".pptx",

            "text/plain" => ".txt",
            "text/csv" => ".csv",
            "text/html" => ".html",
            "application/json" => ".json",
            "application/xml" => ".xml",

            "application/zip" => ".zip",
            "application/x-7z-compressed" => ".7z",
            "application/x-rar-compressed" => ".rar",
            "application/gzip" => ".gz",

            "audio/mpeg" => ".mp3",
            "audio/wav" => ".wav",
            "audio/ogg" => ".ogg",

            "video/mp4" => ".mp4",
            "video/x-msvideo" => ".avi",
            "video/x-matroska" => ".mkv",

            _ => string.Empty
        };
    }
}