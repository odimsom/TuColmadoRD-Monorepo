namespace TuColmadoRD.Infrastructure.CrossCutting.Configuration;

public class OutboxOptions
{
    public const string SectionName = "Outbox";
    public int PollingIntervalSeconds { get; set; } = 15;
    public int BatchSize { get; set; } = 50;
    public int MaxRetries { get; set; } = 5;
    public string CloudSyncBaseUrl { get; set; } = string.Empty;
    public int CatalogSyncIntervalMinutes { get; set; } = 30;
    public int InventorySyncIntervalMinutes { get; set; } = 30;
}

public class RetentionOptions
{
    public const string SectionName = "Retention";
    public int RetentionDays { get; set; } = 7;
    public bool RunAtStartup { get; set; } = true;
    public int? RunAtLocalHour { get; set; }
}
