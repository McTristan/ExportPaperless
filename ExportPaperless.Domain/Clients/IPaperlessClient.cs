using ExportPaperless.Domain.Entities;

namespace ExportPaperless.Domain.Clients;

public interface IPaperlessClient
{
    Task<List<PaperlessDocument>> GetDocuments(DateTime from, DateTime to, List<string> includeTags,
        List<string> excludeTags, List<string> includeDocumentTypes, List<string> includeCustomFields,
        List<string> includeCorrespondents, CancellationToken cancellationToken);

    Task<List<PaperlessDocument>> GetDocumentsFromView(int viewId, CancellationToken cancellationToken);
    Task<SavedView> GetSavedView(int viewId, CancellationToken cancellationToken);
    Task<List<SavedView>> GetSavedViews(CancellationToken cancellationToken);
    Task<byte[]> CreateZipWithDocuments(List<PaperlessDocument> docs, Stream excelStream, CancellationToken cancellationToken);
}