using ExportPaperless.Domain.Clients;
using ExportPaperless.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExportPaperless.Rest.Controller;

[ApiController]
[Route("api/[controller]")]
public class ExportController(IPaperlessClient client, IExcelExportService excelService, IZipService zipService)
    : ControllerBase
{
    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] DateTime from, [FromQuery] DateTime to,
        [FromQuery] List<string> includeTags, [FromQuery] List<string> excludeTags,
        [FromQuery] List<string> includeDocumentTypes,
        [FromQuery] List<string> fields, CancellationToken cancellationToken)
    {
        var documents = await client.GetDocuments(from, to, includeTags, excludeTags, includeDocumentTypes, fields, cancellationToken);
        var excelStream = excelService.GenerateExcel(documents, fields);
        var zipBytes = await zipService.CreateZipWithDocuments(documents, excelStream, cancellationToken);

        return File(zipBytes, "application/zip", "export.zip");
    }
}