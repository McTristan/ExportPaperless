using ExportPaperless.Domain.Clients;
using ExportPaperless.Domain.Services;
using ExportPaperless.Rest.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExportPaperless.Rest.Controller;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = "ApiKeyScheme")]
public class ExportController(IExportPaperlessService exportPaperlessService, ITokenProvider tokenProvider, IStorageService storageService)
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
    
    [HttpPost("store")]
    public async Task<ActionResult<string>> StoreQueryResultWithDocuments(QueryDto queryDto, CancellationToken cancellationToken)
    {
        SetClientToken();
        
        var zipBytes = await exportPaperlessService.ExportByQuery(queryDto.From, queryDto.To, queryDto.IncludeTags,
            queryDto.ExcludeTags, queryDto.IncludeDocumentTypes, queryDto.IncludeCustomFields,
            queryDto.IncludeCorrespondents, cancellationToken);
        var fingerprint = await storageService.StoreFile(GenerateExportFileName("documents"), zipBytes, cancellationToken);
        return CreateDownloadActionResult(fingerprint);
    }
}