using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Api.Common.Exceptions;
using Api.Common.Utilities;
using Api.Domain;
using Api.Domain.ModelSettings;
using Api.DTOs.UploadFileService;
using Microsoft.Extensions.Options;

namespace Api.Services.UploadFileService;

public class S3FileUploadService : BaseUploadFileService, IFileUploadService
{
    private readonly AmazonS3Client _s3Client;
    private readonly string _bucketName;
    private readonly IFileDeleter _fileDeleter;

    public S3FileUploadService(IOptions<S3Settings> settings,
        Context context,
        IUploadTransactionManager uploadTransactionManager,
        IFileDeleter fileDeleter) : base(
        context, uploadTransactionManager)
    {
        _fileDeleter = fileDeleter;
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
            Guid fmKey;

            var fileName = $"{prefix}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = fileName,
                InputStream = file.OpenReadStream(),
                ContentType = file.ContentType
            };

            await _s3Client.PutObjectAsync(request);

            try
            {
                var dto = new AddFileManagementDto()
                {
                    FileName = file.FileName,
                    S3Key = fileName,
                    FileType = file.ContentType,
                    FileSize = file.Length / 1024f / 1024f,
                    UploadBy = null,
                    RelatedObjectId = null,
                    RelatedObjectType = null
                };
                var fileM = await AddFileManagement(dto, false);
                fmKey = fileM.Id;
            }
            catch (Exception)
            {
                await _fileDeleter.DeleteFileAsync(fileName);
                throw;
            }

            UploadTransactionManager.TrackUploadedFile(fileName);
            return new UploadFileResponse()
            {
                Key = fmKey,
                S3Key = fileName,
                ContentType = file.ContentType,
                Size = file.Length,
                S3Url = await GeneratePreSignedDownloadUrlAsync(fileName, 30)
            };
        }
        catch (AmazonS3Exception)
        {
            throw new S3Exception(ErrorMessages.FileUploadError);
        }
    }

    public async Task<UploadFileResponse> UploadStreamAsync(Stream stream, string fileName, string contentType,
        string prefix = "")
    {
        try
        {
            long streamLength = 0;
            if (stream.CanSeek) streamLength = stream.Length;

            Guid fmKey;
            var key = $"{prefix}/{Guid.NewGuid()}{Path.GetExtension(fileName)}";

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = stream,
                ContentType = contentType
            };

            await _s3Client.PutObjectAsync(request);


            try
            {
                var dto = new AddFileManagementDto()
                {
                    FileName = fileName,
                    S3Key = key,
                    FileType = contentType,
                    FileSize = streamLength / 1024f / 1024f,
                    UploadBy = Guid.Empty,
                    RelatedObjectId = Guid.Empty,
                    RelatedObjectType = null
                };
                var fileM = await AddFileManagement(dto, false);
                fmKey = fileM.Id;
            }
            catch (Exception)
            {
                await _fileDeleter.DeleteFileAsync(key);
                throw;
            }

            UploadTransactionManager.TrackUploadedFile(key);
            var response = new UploadFileResponse()
            {
                Key = fmKey,
                S3Key = key,
                ContentType = contentType,
                Size = streamLength,
                S3Url = await GeneratePreSignedDownloadUrlAsync(key, 30)
            };

            return response;
        }
        catch (AmazonS3Exception)
        {
            throw new S3Exception(ErrorMessages.FileUploadError);
        }
    }

    public async Task<bool> DeleteFileManagementAsync(Guid id)
    {
        try
        {
            await DeleteFileManagement(id);
            return true;
        }
        catch (AmazonS3Exception)
        {
            throw new S3Exception(ErrorMessages.FileDeleteError);
        }
    }

    public async Task<bool> DeleteFileManagementAsync(List<Guid> ids)
    {
        foreach (var id in ids) await DeleteFileManagementAsync(id);

        return true;
    }

    public async Task<PreSignedUrlResponse> GeneratePreSignedUploadUrlAsync(PreSignedUrlRequest request,
        int expirationMinutes = 60)
    {
        try
        {
            Guid fmKey;
            var key = $"{request.Prefix}/{Guid.NewGuid()}{Path.GetExtension(request.FileName)}";

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

            try
            {
                var dto = new AddFileManagementDto()
                {
                    FileName = request.FileName,
                    S3Key = key,
                    FileType = request.ContentType,
                    FileSize = request.FileSize / 1024f / 1024f,
                    UploadBy = null,
                    RelatedObjectId = null,
                    RelatedObjectType = null
                };
                var fileM = await AddFileManagement(dto, true);
                fmKey = fileM.Id;
            }
            catch (Exception)
            {
                throw new S3Exception($"Error generating pre-signed upload URL");
            }

            return new PreSignedUrlResponse()
            {
                PreSignedUrl = preSignedUrl,
                FileKey = fmKey,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
            };
        }
        catch (AmazonS3Exception)
        {
            throw new S3Exception(ErrorMessages.GeneratePreSignUploadUrlError);
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
        catch (AmazonS3Exception)
        {
            throw new S3Exception(ErrorMessages.GeneratePreSignDownloadUrlError);
        }
    }

    public async Task<string> GeneratePreSignedDownloadUrlAsync(Guid fileManagementKey, int expirationMinutes = 60)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = await GetS3Key(fileManagementKey),
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes)
            };

            return await _s3Client.GetPreSignedURLAsync(request);
        }
        catch (AmazonS3Exception)
        {
            throw new S3Exception(ErrorMessages.GeneratePreSignDownloadUrlError);
        }
    }

    public async Task<UploadFileResponse> CopyObjectAsync(Guid id, string prefix)
    {
        try
        {
            var fileManagement = await GetFileManagement(id);
            Guid fmKey;
            var key = $"{prefix}/{Guid.NewGuid()}{Path.GetExtension(fileManagement.S3Key)}";

            await _s3Client.CopyObjectAsync(_bucketName,
                fileManagement.S3Key,
                _bucketName,
                key);
            try
            {
                var dto = new AddFileManagementDto()
                {
                    FileName = fileManagement.FileName,
                    S3Key = key,
                    FileType = fileManagement.FileType,
                    FileSize = fileManagement.FileSize,
                    UploadBy = Guid.Empty,
                    RelatedObjectId = Guid.Empty,
                    RelatedObjectType = null
                };
                var fileM = await AddFileManagement(dto, false);
                fmKey = fileM.Id;
            }
            catch (Exception)
            {
                await _fileDeleter.DeleteFileAsync(key);
                throw;
            }

            UploadTransactionManager.TrackUploadedFile(key);
            var response = new UploadFileResponse()
            {
                Key = fmKey,
                S3Key = key,
                ContentType = fileManagement.FileType,
                Size = fileManagement.FileSize,
                S3Url = await GeneratePreSignedDownloadUrlAsync(key, 30)
            };

            return response;
        }
        catch (AmazonS3Exception)
        {
            throw new S3Exception(ErrorMessages.FileUploadError);
        }
    }

    public async Task RollBackAsync()
    {
        await UploadTransactionManager.RollbackAsync();
    }
}