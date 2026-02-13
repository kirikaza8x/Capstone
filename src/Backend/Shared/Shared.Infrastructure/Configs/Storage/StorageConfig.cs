namespace Shared.Infrastructure.Configs.Storage;

public sealed class StorageConfig
{
    public string Endpoint { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string Region { get; set; } = "us-east-1";
    public bool UseSSL { get; set; } = true;
    public string? PublicUrl { get; set; }
}