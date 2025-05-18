namespace ExportPaperless.Domain.Services;

public interface IExportPaperlessService
{
    public Task<byte[]> ExportSavedView(int viewId, CancellationToken cancellationToken);

    Task<byte[]> ExportByQuery(
        DateTime? from,
        DateTime? to,
        List<string>? includeTags,
        List<string>? excludeTags,
        List<string>? includeDocumentTypes,
        List<string>? includeCustomFields,
        List<string>? includeCorrespondents,
        CancellationToken cancellationToken);

    Task<byte[]> ExportSavedViewMetadata(int viewId, CancellationToken cancellationToken);
}