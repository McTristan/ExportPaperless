using ExportPaperless.Domain.Services;
using ExportPaperless.Mcp.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExportPaperless.Mcp;

public static class ServiceCollectionExtensions
{
    public static void AddMcp(this IServiceCollection services, IConfiguration configuration)
    {
        var mcpConfigurationService = new McpConfigurationService(configuration);
        services.AddTransient<IMcpConfigurationService, McpConfigurationService>(b => mcpConfigurationService);
        services.AddSingleton(_ =>
        {
            var httpClient = new HttpClient { BaseAddress = mcpConfigurationService.ExportPaperlessApiUrl };
            if (!string.IsNullOrEmpty(mcpConfigurationService.ExportPaperlessApiToken))
            {
                httpClient.DefaultRequestHeaders.Add("x-api-key", mcpConfigurationService.ExportPaperlessApiToken);
            }

            return httpClient;
        });

    }
}