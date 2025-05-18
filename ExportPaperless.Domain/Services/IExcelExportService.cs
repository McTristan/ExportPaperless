using ExportPaperless.Domain.Entities;

namespace ExportPaperless.Domain.Services;

public interface IExcelExportService
{
    MemoryStream GenerateExcel(List<PaperlessDocument> documents, List<string>? customFields);
    MemoryStream GenerateExcel(List<PaperlessDocument> documents, SavedView view);
}
