using System.IO.Compression;
using System.Net.Http.Headers;
using ExportPaperless.Domain.Entities;
using ExportPaperless.Domain.Services;
using Microsoft.AspNetCore.Http;

namespace ExportPaperless.PaperlessApi.Services;

public class ZipService : IZipService
{
    private readonly HttpClient _httpClient;

    public ZipService(IHttpContextAccessor contextAccessor, IPaperlessConfigurationService configurationService, IHttpClientFactory clientFactory)
    {
        _httpClient = clientFactory.CreateClient("Paperless");
        var token = contextAccessor.HttpContext?.Request.Headers["x-api-key"];
        if (string.IsNullOrEmpty(token))
        {
            token = configurationService.Token;
        }
        
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", token);
    }

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