using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Api.Domain.ModelSettings;
using Api.DTOs.UploadFileService;
using Microsoft.Extensions.Options;

namespace Api.Services.UploadFileService;

public class S3FileUploadService : IFileUploadService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public S3FileUploadService(IOptions<S3Settings> settings)
    {
        _bucketName = settings.Value.BucketName;
        var s3Config = new AmazonS3Config
        {
            RegionEndpoint = RegionEndpoint.APSoutheast1,
            ForcePathStyle = true
        };

        _s3Client = new AmazonS3Client(
            settings.Value.AccessKey,
            settings.Value.SecretKey,
            s3Config
        );
    }

    public async Task<UploadFileResponse> UploadFileAsync(IFormFile file, string prefix = "")
    {
        try
        {
            var fileName = $"{prefix}{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = fileName,
                InputStream = file.OpenReadStream(),
                ContentType = file.ContentType,
                CannedACL = S3CannedACL.PublicRead
            };

            await _s3Client.PutObjectAsync(request);
            
            return new UploadFileResponse()
            {
                Key = fileName,
                ContentType = file.ContentType,
                Size = file.Length,
                S3Url = $"https://{_bucketName}.s3.amazonaws.com/{fileName}",
            };
        }
        catch (AmazonS3Exception ex)
        {
            throw new Exception($"Error uploading file to S3: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteFileAsync(string key)
    {
        try
        {
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(deleteRequest);
            return true;
        }
        catch (AmazonS3Exception ex)
        {
            throw new Exception($"Error deleting file from S3: {ex.Message}", ex);
        }
    }

    public async Task<PreSignedUrlResponse> GeneratePreSignedUploadUrlAsync(PreSignedUrlRequest request, int expirationMinutes = 60)
    {
        try
        {
            var key = $"{request.Prefix}{Guid.NewGuid()}{Path.GetExtension(request.FileName)}";

            var preSignedUrlRequest = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = key,
                Verb = HttpVerb.PUT,
                ContentType = request.ContentType,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Headers =
                {
                    ContentLength = request.FileSize
                }
            };

            var preSignedUrl = await _s3Client.GetPreSignedURLAsync(preSignedUrlRequest);

            return new PreSignedUrlResponse()
            {
                PreSignedUrl = preSignedUrl,
                FileKey = key,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
            };
        }
        catch (AmazonS3Exception ex)
        {
            throw new Exception($"Error generating pre-signed upload URL: {ex.Message}", ex);
        }
    }

    public async Task<string> GeneratePreSignedDownloadUrlAsync(string key, int expirationMinutes = 60)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = key,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes)
            };

            return await _s3Client.GetPreSignedURLAsync(request);
        }
        catch (AmazonS3Exception ex)
        {
            throw new Exception($"Error generating pre-signed download URL: {ex.Message}", ex);
        }
    }
}