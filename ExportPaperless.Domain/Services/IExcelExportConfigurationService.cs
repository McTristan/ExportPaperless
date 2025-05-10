namespace ExportPaperless.Domain.Services;

public interface IExcelExportConfigurationService
{
    public string DateFormat { get; }
    public string NumberFormat { get; }
    public bool StripCurrency { get; }
}