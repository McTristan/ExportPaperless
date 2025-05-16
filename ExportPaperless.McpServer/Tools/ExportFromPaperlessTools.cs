using System.ComponentModel;
using System.Net.Http.Headers;
using ExportPaperless.Domain.Services;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;
using Serilog;

namespace ExportPaperless.McpServer.Tools;

[McpServerToolType]
public class ExportFromPaperlessTools(IMcpConfigurationService mcpConfigurationService)
{
    [McpServerTool(Name = "exportSavedViewAsZip"), Description("Exports a saved view from paperless into a .zip file with an excel metadata file and pdf documents")]
    public async Task<IEnumerable<AIContent>> ExportSavedViewAsZip(
        IExportPaperlessService exportPaperlessService,
        [Description("The saved view Id to export to a zip file")] int viewId,
        CancellationToken cancellationToken)
    {
        try
        {
            var zipBytes = await exportPaperlessService.ExportSavedView(viewId, cancellationToken);
            
            // Einen eindeutigen Dateinamen erzeugen
            var fileName = $"SavedView_{viewId}_{DateTime.UtcNow:yyyyMMddHHmmss}.zip";
        
            // Datei per Multipart Form Upload an die angegebene URL hochladen
            using var httpClient = new HttpClient();
            using var multipartContent = new MultipartFormDataContent();

            if (!string.IsNullOrEmpty(mcpConfigurationService.Token))
            {
                // API-Key als Header hinzufügen
                httpClient.DefaultRequestHeaders.Add("x-api-key", mcpConfigurationService.Token);
            }

            // Dateiinhalt hinzufügen
            using var fileContent = new ByteArrayContent(zipBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
        
            // Den Dateinamen als Teil des Multipart-Formulars angeben
            multipartContent.Add( fileContent, "data", fileName);
        
            var downloadUri = new Uri(mcpConfigurationService.DownloadUri, fileName);
            // Upload durchführen
            var response = await httpClient.PostAsync(mcpConfigurationService.UploadUri, multipartContent, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return 
                [
                    new TextContent("Die ZIP-Datei wurde erfolgreich erstellt und hochgeladen."),
                    new TextContent("Hier ist die Download-Url:"),
                    new UriContent(downloadUri, "application/zip"),
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
    
    [McpServerTool(Name = "echo"), Description("Echoes the message back to the client.")]
    public static string Echo(string message) => $"Echo: {message}";
}

