using ExportPaperless.Domain.Services;
using ExportPaperless.PaperlessApi;
using ExportPaperless.Services;

namespace ExportPaperless;

public static class Registry
{
    public static void AddServices(this IServiceCollection services)
    {
        var configuration = Configuration.GetStandardConfiguration();
        services.AddSingleton(configuration);
        services.AddPaperlessApi(configuration);
        services.AddTransient<IExcelExportService, ExcelExportService>();
        services.AddTransient<IExcelExportConfigurationService, ExcelExportConfigurationService>();

        services.AddControllers();
    }
}