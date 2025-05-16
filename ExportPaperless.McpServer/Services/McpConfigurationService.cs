using ExportPaperless.Domain.Services;
using Microsoft.Extensions.Configuration;

namespace ExportPaperless.McpServer.Services;

public class McpConfigurationService(IConfiguration configuration): IMcpConfigurationService
{
    private readonly IConfigurationSection _section = configuration.GetSection("MCP");

    public Uri DownloadUri
    {
        get
        {
            var downloadUrl = _section["DOWNLOAD_URL"];
            if (string.IsNullOrEmpty(downloadUrl))
            {
                throw new InvalidOperationException(
                    "DOWNLOAD_URL variable is not set (appsettings.json), you can also use an environment variable in the format: MCP__DOWNLOAD_URL=http://localhost:8000");
            }

            return new Uri(downloadUrl);
        }
    }
    
    public Uri UploadUri
    {
        get
        {
            var uploadUrl = _section["UPLOAD_URL"];
            if (string.IsNullOrEmpty(uploadUrl))
            {
                throw new InvalidOperationException(
                    "UPLOAD_URL variable is not set (appsettings.json), you can also use an environment variable in the format: MCP__UPLOAD_URL=http://localhost:8000");
            }

            return new Uri(uploadUrl);
        }
    }

    public string? Token => _section["API_TOKEN"];
    
}