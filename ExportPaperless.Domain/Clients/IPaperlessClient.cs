using ExportPaperless.Domain.Entities;

namespace ExportPaperless.Domain.Clients;

public interface IPaperlessClient
{
    Task<List<PaperlessDocument>> GetDocuments(DateTime from, DateTime to, List<string> includeTags,
        List<string> excludeTags, List<string> includeDocumentTypes, List<string> fields,
        CancellationToken cancellationToken);
}