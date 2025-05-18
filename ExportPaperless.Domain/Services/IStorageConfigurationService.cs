namespace ExportPaperless.Domain.Services;

public interface IStorageConfigurationService
{
    string StoragePath { get; }
    TimeSpan MaxRetentionTime { get; }
}