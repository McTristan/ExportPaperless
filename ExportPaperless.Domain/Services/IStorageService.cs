namespace ExportPaperless.Domain.Services;

public interface IStorageService
{
    Task<string> StoreFile(string fileName, byte[] file, CancellationToken cancellationToken);
    Task<(byte[] Content, string FileName)> GetFile(string fingerprint, bool deleteAfterDownload, CancellationToken cancellationToken);
}