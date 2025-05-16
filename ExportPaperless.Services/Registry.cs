using ExportPaperless.Domain.Services;
using ExportPaperless.Excel;
using ExportPaperless.PaperlessApi;
using ExportPaperless.Services.Services;
using ExportPaperless.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace ExportPaperless.Services;

public static class Registry
{
    public static void AddServices(this IServiceCollection services)
    {
        var configuration = Configuration.GetStandardConfiguration();
        services.AddSingleton(configuration);
        services.AddPaperlessApi(configuration);
        services.AddStorage();
        services.AddExcel();
        services.AddTransient<IExportPaperlessService, ExportPaperlessService>();
    }
}