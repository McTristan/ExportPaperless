using System.Security.Cryptography;
using ExportPaperless.Domain.Services;

namespace ExportPaperless.Storage.Services;

public class StorageService(IStorageConfigurationService configurationService): IStorageService
{
    public async Task<string> StoreFile(string fileName, byte[] file, CancellationToken cancellationToken)
    {
        // Hash des Dateiinhalts
        var contentHashBytes = SHA256.HashData(file);
        var contentHash = BitConverter.ToString(contentHashBytes).Replace("-", "").ToLowerInvariant();

        // Hash des Dateinamens
        var fileNameBytes = System.Text.Encoding.UTF8.GetBytes(fileName);
        var fileNameHashBytes = SHA256.HashData(fileNameBytes);
        var fileNameHash = BitConverter.ToString(fileNameHashBytes).Replace("-", "").ToLowerInvariant();

        var fingerprint = $"{contentHash}-{fileNameHash}";

        var storageFileName = Path.Combine(configurationService.StoragePath, $"{fingerprint}_{fileName}");
        await File.WriteAllBytesAsync(storageFileName, file, cancellationToken);
        return fingerprint;
    }

    public async Task<(byte[] Content, string FileName)> GetFile(string fingerprint, bool deleteAfterDownload, CancellationToken cancellationToken)
    {
        var files = Directory.EnumerateFiles(configurationService.StoragePath, $"{fingerprint}_*").ToList();
        var file = files.FirstOrDefault();
        if (file == null)
        {
            throw new FileNotFoundException();
        }
    
        try
        {
            var fileName = Path.GetFileName(file).Replace($"{fingerprint}_", "");
            var content = await File.ReadAllBytesAsync(file, cancellationToken);
            return (content, fileName);
        }
        finally
        {
            if (deleteAfterDownload)
            {
                File.Delete(file);
            }
        }
    }
}