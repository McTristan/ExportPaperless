using ExportPaperless.Domain.Services;
using Microsoft.Extensions.Configuration;

namespace ExportPaperless.Storage.Services;

public class StorageConfigurationService(IConfiguration configuration): IStorageConfigurationService
{
    private readonly IConfigurationSection _section = configuration.GetSection("STORAGE");

    public string StoragePath
    {
        get
        {
            var storagePath = _section["DATA_PATH"];
            if (string.IsNullOrEmpty(storagePath))
            {
                storagePath = Path.Combine(Path.GetTempPath(), "paperless-export");
            }
            
            Directory.CreateDirectory(storagePath);
            
            return storagePath;
        }
    }

    public TimeSpan MaxRetentionTime
    {
        get
        {
            var maxRetentionTimeInDays = _section["MAX_RETENTION_TIME_IN_DAYS"];
            if (string.IsNullOrEmpty(maxRetentionTimeInDays))
            {
                maxRetentionTimeInDays = "3";
            }
            
            return TimeSpan.FromDays(int.Parse(maxRetentionTimeInDays));
        }
    } 
}