namespace AI.Infrastructure.Configs;

public class GeminiConfig
{
    public string ApiKey { get; set; } = default!;
    public string Model { get; set; } = "gemini-2.5-flash";
    public string? SystemInstruction { get; set; }
}
