namespace ExportPaperless.Domain.Services;

public interface IMcpConfigurationService
{
    public Uri DownloadUri { get; }
    public Uri UploadUri { get; }
    public string? Token { get; }
}