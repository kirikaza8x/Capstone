using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Application.Abstractions.Storage;
using Shared.Infrastructure.Configs.Storage;

namespace Shared.Infrastructure.Service.Storage;

public sealed class MinioStorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly StorageConfig _config;
    private readonly ILogger<MinioStorageService> _logger;
    private bool _bucketChecked;

    public MinioStorageService(
        IOptions<StorageConfig> config,
        ILogger<MinioStorageService> logger)
    {
        _config = config.Value;
        _logger = logger;

        // Ensure endpoint has protocol
        var endpoint = _config.Endpoint;
        if (!endpoint.StartsWith("http://") && !endpoint.StartsWith("https://"))
        {
            endpoint = _config.UseSSL ? $"https://{endpoint}" : $"http://{endpoint}";
        }

        var s3Config = new AmazonS3Config
        {
            ServiceURL = endpoint,
            ForcePathStyle = true,
            UseHttp = !_config.UseSSL
        };

        _s3Client = new AmazonS3Client(
            _config.AccessKey,
            _config.SecretKey,
            s3Config);
    }

    public async Task<string> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string? folder = null,
        CancellationToken cancellationToken = default)
    {
        // Ensure bucket exists
        await EnsureBucketExistsAsync(cancellationToken);

        var objectKey = GenerateObjectKey(fileName, folder);

        try
        {
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = fileStream,
                Key = objectKey,
                BucketName = _config.BucketName,
                ContentType = contentType,
                CannedACL = S3CannedACL.PublicRead
            };

            using var transferUtility = new TransferUtility(_s3Client);
            await transferUtility.UploadAsync(uploadRequest, cancellationToken);

            _logger.LogInformation("File uploaded successfully: {ObjectKey}", objectKey);

            return GetPublicUrl(objectKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file: {FileName}", fileName);
            throw;
        }
    }

    public async Task<string> UploadAsync(
        byte[] fileBytes,
        string fileName,
        string contentType,
        string? folder = null,
        CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream(fileBytes);
        return await UploadAsync(stream, fileName, contentType, folder, cancellationToken);
    }

    public async Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        var objectKey = ExtractObjectKeyFromUrl(fileUrl);

        try
        {
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _config.BucketName,
                Key = objectKey
            };

            await _s3Client.DeleteObjectAsync(deleteRequest, cancellationToken);
            _logger.LogInformation("File deleted successfully: {ObjectKey}", objectKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file: {ObjectKey}", objectKey);
            throw;
        }
    }

    public async Task<Stream?> DownloadAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        var objectKey = ExtractObjectKeyFromUrl(fileUrl);

        try
        {
            var response = await _s3Client.GetObjectAsync(
                _config.BucketName,
                objectKey,
                cancellationToken);

            return response.ResponseStream;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("File not found: {ObjectKey}", objectKey);
            return null;
        }
    }

    public string GetPublicUrl(string objectKey)
    {
        if (!string.IsNullOrEmpty(_config.PublicUrl))
        {
            return $"{_config.PublicUrl.TrimEnd('/')}/{objectKey}";
        }

        return $"{_config.Endpoint}/{_config.BucketName}/{objectKey}";
    }

    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken = default)
    {
        if (_bucketChecked) return;

        try
        {
            var bucketExists = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(
                _s3Client,
                _config.BucketName);

            if (!bucketExists)
            {
                _logger.LogInformation("Creating bucket: {BucketName}", _config.BucketName);

                await _s3Client.PutBucketAsync(new PutBucketRequest
                {
                    BucketName = _config.BucketName,
                    UseClientRegion = true
                }, cancellationToken);

                // Set bucket policy for public read (optional)
                var policy = $$"""
                {
                    "Version": "2012-10-17",
                    "Statement": [
                        {
                            "Effect": "Allow",
                            "Principal": "*",
                            "Action": ["s3:GetObject"],
                            "Resource": ["arn:aws:s3:::{{_config.BucketName}}/*"]
                        }
                    ]
                }
                """;

                await _s3Client.PutBucketPolicyAsync(new PutBucketPolicyRequest
                {
                    BucketName = _config.BucketName,
                    Policy = policy
                }, cancellationToken);

                _logger.LogInformation("Bucket created successfully: {BucketName}", _config.BucketName);
            }

            _bucketChecked = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ensure bucket exists: {BucketName}", _config.BucketName);
            throw;
        }
    }

    private static string GenerateObjectKey(string fileName, string? folder)
    {
        var sanitizedFileName = SanitizeFileName(fileName);
        var uniqueFileName = $"{Guid.NewGuid():N}_{sanitizedFileName}";

        return string.IsNullOrEmpty(folder)
            ? uniqueFileName
            : $"{folder.Trim('/')}/{uniqueFileName}";
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
    }

    private string ExtractObjectKeyFromUrl(string fileUrl)
    {
        if (Uri.TryCreate(fileUrl, UriKind.Absolute, out var uri))
        {
            var path = uri.AbsolutePath.TrimStart('/');
            if (path.StartsWith(_config.BucketName))
            {
                path = path[(_config.BucketName.Length + 1)..];
            }
            return path;
        }
        return fileUrl;
    }
}