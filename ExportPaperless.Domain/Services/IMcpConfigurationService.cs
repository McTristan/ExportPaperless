namespace ExportPaperless.Domain.Services;

public interface IMcpConfigurationService
{
    Uri ExportPaperlessApiUrl { get; }
    string? ExportPaperlessApiToken { get; }
}