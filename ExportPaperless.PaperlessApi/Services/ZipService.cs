using System.IO.Compression;
using ExportPaperless.Domain.Entities;
using ExportPaperless.Domain.Services;

namespace ExportPaperless.PaperlessApi.Services;

public class ZipService(IHttpClientFactory clientFactory) : IZipService
{
    private readonly HttpClient _httpClient = clientFactory.CreateClient("Paperless");

    public async Task<byte[]> CreateZipWithDocuments(List<PaperlessDocument> docs, Stream excelStream, CancellationToken cancellationToken)
    {
        using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
        {
            var excelEntry = archive.CreateEntry("metadata.xlsx");
            await using (var entryStream = excelEntry.Open())
            {
                await excelStream.CopyToAsync(entryStream, cancellationToken);
            }

            foreach (var doc in docs)
            {
                var response = await _httpClient.GetAsync($"documents/{doc.Id}/download/", cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    continue;
                }
                var fileEntry = archive.CreateEntry(doc.FileName);
                await using var docStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                await using var entryStream = fileEntry.Open();
                await docStream.CopyToAsync(entryStream, cancellationToken);
            }
        }
        return ms.ToArray();
    }
    
}