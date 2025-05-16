using ExportPaperless.Domain.Services;
using Microsoft.Extensions.Configuration;

namespace ExportPaperless.Excel.Services;

public class ExcelExportConfigurationService(IConfiguration configuration) : IExcelExportConfigurationService
{
    private readonly IConfigurationSection _section = configuration.GetSection("EXCEL");
    public string DateFormat
    {
        get
        {
            var dateFormat = _section["DATE_FORMAT"];
            return dateFormat ?? "yyyy-MM-dd";
        }
    }

    public string NumberFormat
    {
        get
        {
            var numberFormat = _section["NUMBER_FORMAT"];
            return numberFormat ?? "0.00";
        }
    }

    public bool StripCurrency
    {
        get
        {
            var stripCurrency = _section["STRIP_CURRENCY"];
            if (string.IsNullOrEmpty(stripCurrency))
            {
                return false;
            }
            return stripCurrency == "true";
        }
    }
    
}
