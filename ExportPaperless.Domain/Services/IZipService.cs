using ExportPaperless.Domain.Entities;

namespace ExportPaperless.Domain.Services;

public interface IZipService
{
    Task<byte[]> CreateZipWithDocuments(List<PaperlessDocument> docs, Stream excelStream, CancellationToken cancellationToken);
}