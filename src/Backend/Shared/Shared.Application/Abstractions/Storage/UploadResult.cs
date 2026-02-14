namespace Shared.Application.Abstractions.Storage;

public sealed record UploadResult(
    string Url,
    string ObjectKey,
    string FileName,
    long Size,
    string ContentType);