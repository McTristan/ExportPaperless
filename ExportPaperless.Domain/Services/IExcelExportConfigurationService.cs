namespace ExportPaperless.Domain.Services;

public interface IExcelExportConfigurationService
{
    public string DateFormat { get; }
    public string NumberFormat { get; }
    public bool StripCurrency { get; }
    public string ReplacementTitle { get; }
    public string ReplacementCreated { get; }
    public string ReplacementNotes { get; }
    public string ReplacementTags { get; }
    public string ReplacementDocumentType { get; }
    public string ReplacementCorrespondent { get; }
    public string ReplacementPageCount { get; }
    public string ReplacementFileName { get; }
    public string ReplacementUrl { get; }
}