using System.ComponentModel;
using System.Text.Json;
using ExportPaperless.PaperlessApi.DataContracts;
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
                new TextContent("Fehler beim Abrufen der gespeicherten Ansichten."),
                new TextContent($"Statuscode: {response.StatusCode}"),
                new TextContent($"Fehlermeldung: {await response.Content.ReadAsStringAsync(cancellationToken)}"),
                new TextContent($"Base Url: {httpClient.BaseAddress}, Request Url: {response.RequestMessage?.RequestUri}")
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
        try
        {
            var response = await httpClient.PostAsync($"/api/SavedView/store/{viewId}", null, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return
                [
                    new TextContent($"Fehler beim Abrufen der gespeicherten Ansicht {viewId}."),
                    new TextContent($"Statuscode: {response.StatusCode}"),
                    new TextContent($"Fehlermeldung: {await response.Content.ReadAsStringAsync(cancellationToken)}")
                ];
            }

            // Location-URL aus dem Header extrahieren
            var locationUrl = response.Headers.Location?.ToString();

            if (!string.IsNullOrEmpty(locationUrl))
            {
                return
                [
                    new TextContent("Die ZIP-Datei wurde erfolgreich erstellt und hochgeladen."),
                    new TextContent("Hier ist die Download-Url:"),
                    new UriContent(locationUrl, "application/zip"),
                ];
            }

            return
            [
                new TextContent("Die ZIP-Datei wurde erstellt, aber der Upload ist fehlgeschlagen."),
                new TextContent($"Statuscode: {response.StatusCode}"),
                new TextContent($"Fehlermeldung: {await response.Content.ReadAsStringAsync(cancellationToken)}")
            ];
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "ExportSavedViewAsZip failed abnormally");
            throw;
        }
    }
}