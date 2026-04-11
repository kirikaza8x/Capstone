using Microsoft.AspNetCore.Http;
using Shared.Application.Abstractions.Storage;
public class FormFileUpload : IFileUpload
{
    private readonly IFormFile _file;
    public FormFileUpload(IFormFile file) => _file = file;
    public string FileName => _file.FileName;
    public string ContentType => _file.ContentType;
    public long Length => _file.Length;
    public Stream OpenReadStream() => _file.OpenReadStream();
}
