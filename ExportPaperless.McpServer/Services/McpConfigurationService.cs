using ExportPaperless.Domain.Services;
using Microsoft.Extensions.Configuration;

namespace ExportPaperless.McpServer.Services;

public class McpConfigurationService(IConfiguration configuration): IMcpConfigurationService
{
    private readonly IConfigurationSection _section = configuration.GetSection("MCP");
   
    public Uri ExportPaperlessApiUrl
    {
        get
        {
            var exportPaperlessApiUrl = _section["EXPORT_PAPERLESS_API_URL"];
            if (string.IsNullOrEmpty(exportPaperlessApiUrl))
            {
                throw new InvalidOperationException(
                    "EXPORT_PAPERLESS_API_URL variable is not set (appsettings.json), you can also use an environment variable in the format: MCP__EXPORT_PAPERLESS_API_URL=http://localhost:8000");
            }

            return new Uri(exportPaperlessApiUrl);
        }
    }

    public string? ExportPaperlessApiToken => _section["EXPORT_PAPERLESS_API_TOKEN"];
}