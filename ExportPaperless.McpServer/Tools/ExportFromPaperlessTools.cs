using System.ComponentModel;
using System.Net.Http.Json;
using System.Runtime.Serialization;
using System.Text.Json;
using ExportPaperless.McpServer.DataContracts;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;
using Serilog;

namespace ExportPaperless.McpServer.Tools;

[McpServerToolType]
public class ExportFromPaperlessTools
{
    [McpServerTool(Name = "listSavedViews"), Description("Lists all saved views from paperless with name and id")]
    public async Task<IEnumerable<AIContent>> ListSavedViews(
        HttpClient httpClient,
        CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync("/api/savedview", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return
            [
                new TextContent("Error collecting saved views from paperless. Please check the log for details."),
                new TextContent($"Status Code: {response.StatusCode}"),
                new TextContent($"Error: {await response.Content.ReadAsStringAsync(cancellationToken)}"),
                new TextContent(
                    $"Base Url: {httpClient.BaseAddress}, Request Url: {response.RequestMessage?.RequestUri}")
            ];
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var savedViewDtos = JsonSerializer.Deserialize<List<SavedViewDto>>(responseContent);

        if (savedViewDtos == null)
        {
            return new List<AIContent>();
        }

        return savedViewDtos.Select(savedViewDto => new TextContent($"{savedViewDto.Id} - {savedViewDto.Name}"))
            .Cast<AIContent>().ToList();
    }

    [McpServerTool(Name = "exportSavedViewAsZip"),
     Description("Exports a saved view from paperless into a .zip file with an excel metadata file and pdf documents")]
    public async Task<IEnumerable<AIContent>> ExportSavedViewAsZip(
        HttpClient httpClient,
        [Description("The saved view Id to export to a zip file")]
        int viewId,
        CancellationToken cancellationToken)
    {
        return await StoreAndReturnUrl(httpClient, new Uri($"/api/SavedView/store/{viewId}", UriKind.Relative),
            "application/zip", cancellationToken);
    }

    [McpServerTool(Name = "exportSavedViewMetadata"),
     Description("Exports a saved view from paperless into a Microsoft Excel metadata file")]
    // public async Task<IEnumerable<AIContent>> ExportSavedViewMetadata(
    public async Task<IEnumerable<AIContent>> ExportSavedViewMetadata(
        HttpClient httpClient,
        [Description("The saved view Id to export as an Microsoft Excel file")]
        int viewId,
        CancellationToken cancellationToken)
    {
        return await StoreAndReturnUrl(httpClient, new Uri($"/api/SavedView/store/metadata/{viewId}", UriKind.Relative), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", cancellationToken);
    }

    [McpServerTool(Name = "exportPaperlessDocumentsByQuery"),
     Description(
         "Exports documents from paperless into a .zip file with an excel metadata file and pdf document using multiple query parameter")]
    public async Task<IEnumerable<AIContent>> ExportPaperlessDocumentsByQuery(
        HttpClient httpClient,
        [Description("The from date filter specifying to list all documents created after or at the given date")]
        DateTime from,
        [Description("The to date filter specifying to list all documents created before or at the given date")]
        DateTime to,
        [Description("The tags to filter documents by, documents include all given tags")]
        List<string> includeTags,
        [Description("The tags to exclude documents by, documents must not have these given tags")]
        List<string> excludeTags,
        [Description("The document types to filter documents by, documents must be of one of the given document types")]
        List<string> includeDocumentTypes,
        [Description("The custom fields to filter documents by, documents must include all of the given custom fields")]
        List<string> includeCustomFields,
        [Description("The correspondents to filter documents by, documents must include one of the given correspondents")]
        List<string> includeCorrespondents,
        CancellationToken cancellationToken)
    {

        var content = new ExportByQueryDto
        {
            From = from,
            To = to,
            IncludeTags = includeTags,
            ExcludeTags = excludeTags,
            IncludeDocumentTypes = includeDocumentTypes,
            IncludeCustomFields = includeCustomFields,
            IncludeCorrespondents = includeCorrespondents
        };
        
        return await StoreAndReturnUrl(httpClient, new Uri($"/api/Export/store", UriKind.Relative), content,
            "application/zip", cancellationToken);
    }

    private static async Task<IEnumerable<AIContent>> StoreAndReturnUrl<T>(HttpClient httpClient, Uri url, T content, string mimeType,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(url, content, cancellationToken);

            return await ExtractLocationUrlFromResponse(url, mimeType, cancellationToken, response);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, $"StoreAndReturnUrl {url} failed abnormally");
            throw;
        }
    }
    
    private static async Task<IEnumerable<AIContent>> StoreAndReturnUrl(HttpClient httpClient, Uri url, string mimeType,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.PostAsync(url, null, cancellationToken);

            return await ExtractLocationUrlFromResponse(url, mimeType, cancellationToken, response);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, $"StoreAndReturnUrl {url} failed abnormally");
            throw;
        }
    }

    private static async Task<IEnumerable<AIContent>> ExtractLocationUrlFromResponse(Uri url, string mimeType, CancellationToken cancellationToken,
        HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            return
            [
                new TextContent($"Error processing request to {url}."),
                new TextContent($"Status Code: {response.StatusCode}"),
                new TextContent($"Error: {await response.Content.ReadAsStringAsync(cancellationToken)}")
            ];
        }

        // Location-URL aus dem Header extrahieren
        var locationUrl = response.Headers.Location?.ToString();

        if (!string.IsNullOrEmpty(locationUrl))
        {
            return
            [
                new UriContent(locationUrl, mimeType),
            ];
        }

        return
        [
            new TextContent("A file was generated but the upload failed."),
            new TextContent($"Status Code: {response.StatusCode}"),
            new TextContent($"Error: {await response.Content.ReadAsStringAsync(cancellationToken)}")
        ];
    }
}