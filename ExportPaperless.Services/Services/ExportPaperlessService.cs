using ExportPaperless.Domain.Clients;
using ExportPaperless.Domain.Services;

namespace ExportPaperless.Services.Services;

public class ExportPaperlessService(IPaperlessClientFactory clientFactory, IExcelExportService excelService)
    : IExportPaperlessService
{
    public async Task<byte[]> ExportByQuery(
        DateTime? from,
        DateTime? to,
        List<string>? includeTags,
        List<string>? excludeTags,
        List<string>? includeDocumentTypes,
        List<string>? includeCustomFields,
        List<string>? includeCorrespondents,
        CancellationToken cancellationToken)
    {
        var client = clientFactory.CreateClient();
        var documents = await client.GetDocuments(from, to, includeTags, excludeTags,
            includeDocumentTypes, includeCustomFields, includeCorrespondents, cancellationToken);
        var excelStream = excelService.GenerateExcel(documents, includeCustomFields);
        return await client.CreateZipWithDocuments(documents, excelStream, cancellationToken);
    }

    public async Task<byte[]> ExportSavedView(int viewId, CancellationToken cancellationToken)
    {
        var client = clientFactory.CreateClient();
        var documents = await client.GetDocumentsFromView(viewId, cancellationToken);
        var savedView = await client.GetSavedView(viewId, cancellationToken);
        var excelStream = excelService.GenerateExcel(documents, savedView);
        return await client.CreateZipWithDocuments(documents, excelStream, cancellationToken);
    }

    public async Task<byte[]> ExportSavedViewMetadata(int viewId, CancellationToken cancellationToken)
    {
        var client = clientFactory.CreateClient();
        var documents = await client.GetDocumentsFromView(viewId, cancellationToken);
        var savedView = await client.GetSavedView(viewId, cancellationToken);
        return excelService.GenerateExcel(documents, savedView).ToArray();
    }
}