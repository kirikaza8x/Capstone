namespace Shared.Application.Abstractions.Storage;

public interface IFileUpload
{
    string FileName { get; }
    string ContentType { get; }
    long Length { get; }
    Stream OpenReadStream();
}
