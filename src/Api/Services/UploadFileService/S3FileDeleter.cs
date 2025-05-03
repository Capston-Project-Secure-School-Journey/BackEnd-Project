using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Api.Common.Exceptions;
using Api.Common.Utilities;
using Api.Domain.ModelSettings;
using Microsoft.Extensions.Options;

namespace Api.Services.UploadFileService;

public class S3FileDeleter : IFileDeleter
{
    private readonly AmazonS3Client _s3Client;
    private readonly string _bucketName;

    public S3FileDeleter(IOptions<S3Settings> settings)
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

    public async Task<bool> DeleteFileAsync(string key)
    {
        try
        {
            if (await FileExist(key))
                return false;
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(deleteRequest);
            return true;
        }
        catch (AmazonS3Exception)
        {
            throw new S3Exception(ErrorMessages.FileDeleteError);
        }
    }

    private async Task<bool> FileExist(string key, bool isThrow = false)
    {
        try
        {
            var metadataRequest = new GetObjectMetadataRequest
            {
                BucketName = _bucketName,
                Key = key
            };
            await _s3Client.GetObjectMetadataAsync(metadataRequest);
            return true;
        }
        catch (AmazonS3Exception)
        {
            if (isThrow)
                throw new S3Exception(ErrorMessages.FileCheckErrorByKey(key));
            return false;
        }
    }
}