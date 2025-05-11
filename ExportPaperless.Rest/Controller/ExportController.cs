using ExportPaperless.Domain.Clients;
using ExportPaperless.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExportPaperless.Rest.Controller;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = "ApiKeyScheme")]
public class ExportController(IPaperlessClient client, IExcelExportService excelService, IZipService zipService)
    : ControllerBase
{
    [HttpGet("")]
    public async Task<IActionResult> Export(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] List<string> includeTags,
        [FromQuery] List<string> excludeTags,
        [FromQuery] List<string> includeDocumentTypes,
        [FromQuery] List<string> includeCustomFields, 
        [FromQuery] List<string> includeCorrespondents,
        CancellationToken cancellationToken)
    {
        var documents = await client.GetDocuments(from, to, includeTags, excludeTags, 
            includeDocumentTypes, includeCustomFields, includeCorrespondents, cancellationToken);
        var excelStream = excelService.GenerateExcel(documents, includeCustomFields);
        var zipBytes = await zipService.CreateZipWithDocuments(documents, excelStream, cancellationToken);

        return File(zipBytes, "application/zip", "export.zip");
    }

    [HttpGet("view/{id}")]
    public async Task<IActionResult> ExportView([FromRoute] int id, CancellationToken cancellationToken)
    {
        var documents = await client.GetDocumentsFromView(id, cancellationToken);
        var savedView = await client.GetSavedView(id, cancellationToken);
        var excelStream = excelService.GenerateExcel(documents, savedView);
        var zipBytes = await zipService.CreateZipWithDocuments(documents, excelStream, cancellationToken);

        return File(zipBytes, "application/zip", "export.zip");
    }
}