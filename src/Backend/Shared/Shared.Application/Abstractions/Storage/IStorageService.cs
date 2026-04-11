namespace Shared.Application.Abstractions.Storage;

public interface IStorageService
{
    Task<string> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string? folder = null,
        CancellationToken cancellationToken = default);

    Task<string> UploadAsync(
        byte[] fileBytes,
        string fileName,
        string contentType,
        string? folder = null,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default);

    Task<Stream?> DownloadAsync(string fileUrl, CancellationToken cancellationToken = default);

    string GetPublicUrl(string objectKey);
}
