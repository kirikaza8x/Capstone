namespace Shared.Infrastructure.Configs;

public class CorsConfig : ConfigBase
{
    // This will bind to the "Cors" section in appsettings.json
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    public bool AllowAnyOrigin { get; set; } = false;
}