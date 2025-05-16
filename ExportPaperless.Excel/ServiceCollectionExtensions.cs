using ExportPaperless.Domain.Services;
using ExportPaperless.Excel.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ExportPaperless.Excel;

public static class ServiceCollectionExtensions
{
    public static void AddExcel(this IServiceCollection services)
    {
        services.AddTransient<IExcelExportService, ExcelExportService>();
        services.AddTransient<IExcelExportConfigurationService, ExcelExportConfigurationService>();

    }
}