namespace Shared.Domain.Common.Options;

public class BulkInsertOptions
{
    public int? BatchSize { get; set; }
    public bool PreserveInsertOrder { get; set; }
    public bool SetOutputIdentity { get; set; }
    public bool UseTempDB { get; set; }
}
