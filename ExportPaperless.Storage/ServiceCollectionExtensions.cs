using ExportPaperless.Domain.Services;
using ExportPaperless.Storage.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ExportPaperless.Storage;

public static class ServiceCollectionExtensions
{
    public static void AddStorage(this IServiceCollection services)
    {
        services.AddTransient<IStorageConfigurationService, StorageConfigurationService>();
        services.AddTransient<IStorageService, StorageService>();
    }
}