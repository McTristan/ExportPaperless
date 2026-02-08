using ExportPaperless.Domain.Services;
using Microsoft.Extensions.Configuration;

namespace ExportPaperless.Excel.Services;

public class ExcelExportConfigurationService(IConfiguration configuration) : IExcelExportConfigurationService
{
    private readonly IConfigurationSection _section = configuration.GetSection("EXCEL");
    public string DateFormat => GetValueWithDefault("DATE_FORMAT", "yyyy-MM-dd");
    public string NumberFormat => GetValueWithDefault("NUMBER_FORMAT", "0.00");
    
    public bool StripCurrency
    {
        get
        {
            var stripCurrency = GetValueWithDefault("STRIP_CURRENCY", "false");
            return stripCurrency == "true";
        }
    }

    public string ReplacementTitle => GetValueWithDefault("REPLACEMENT_TITLE", "Title");
    public string ReplacementCreated => GetValueWithDefault("REPLACEMENT_CREATED", "Created");
    public string ReplacementNotes => GetValueWithDefault("REPLACEMENT_NOTES", "Notes");
    public string ReplacementTags => GetValueWithDefault("REPLACEMENT_TAGS", "Tags");
    public string ReplacementDocumentType => GetValueWithDefault("REPLACEMENT_DOCUMENT_TYPE", "Document Type");
    public string ReplacementCorrespondent => GetValueWithDefault("REPLACEMENT_CORRESPONDENT", "Correspondent");
    public string ReplacementPageCount => GetValueWithDefault("REPLACEMENT_PAGE_COUNT", "Page Count");
    public string ReplacementFileName => GetValueWithDefault("REPLACEMENT_FILE_NAME", "Filename");
    public string ReplacementUrl => GetValueWithDefault("REPLACEMENT_URL", "URL");
    
    private string GetValueWithDefault(string key, string defaultValue)
    {
        var numberFormat = _section[key];
        return numberFormat ?? defaultValue;
    }

}
