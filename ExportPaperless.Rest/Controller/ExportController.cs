using ExportPaperless.Domain.Clients;
using ExportPaperless.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExportPaperless.Rest.Controller;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = "ApiKeyScheme")]
public class ExportController(IExportPaperlessService exportPaperlessService, ITokenProvider tokenProvider)
    : BaseController(tokenProvider)
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
        SetClientToken();

        var zipBytes = await exportPaperlessService.ExportByQuery(from, to, includeTags, excludeTags,
           includeDocumentTypes, includeCustomFields, includeCorrespondents, cancellationToken);
        return File(zipBytes, "application/zip", "export.zip");
    }
}