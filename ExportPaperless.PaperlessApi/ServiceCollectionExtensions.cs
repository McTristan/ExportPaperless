using ExportPaperless.Domain.Clients;
using ExportPaperless.Domain.Services;
using ExportPaperless.PaperlessApi.Clients;
using ExportPaperless.PaperlessApi.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExportPaperless.PaperlessApi;

public static class ServiceCollectionExtensions
{
    public static void AddPaperlessApi(this IServiceCollection services, IConfiguration configuration)
    {
        var paperlessConfigurationService = new PaperlessConfigurationService(configuration);
        services.AddTransient<IPaperlessConfigurationService, PaperlessConfigurationService>(s => paperlessConfigurationService);
        
        services.AddHttpClient("Paperless", client =>
        {
            client.BaseAddress = paperlessConfigurationService.BaseAddress;
        });
        
        services.AddTransient<IPaperlessClient, PaperlessClient>();
        services.AddScoped<ITokenProvider, TokenProvider>();
        services.AddScoped<IPaperlessClientFactory, PaperlessClientFactory>();
    }
}