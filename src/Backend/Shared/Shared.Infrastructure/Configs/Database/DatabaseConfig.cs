namespace Shared.Infrastructure.Configs.Database;

public class DatabaseConfig : ConfigBase
{
    public override string SectionName => "Database";
    public string ConnectionString { get; set; } = string.Empty;
    public int MaxRetryCount { get; set; } = 3;
    public int CommandTimeout { get; set; } = 30;
    public bool EnableDetailedErrors { get; set; } = false;
    public bool EnableSensitiveDataLogging { get; set; } = false;
}
