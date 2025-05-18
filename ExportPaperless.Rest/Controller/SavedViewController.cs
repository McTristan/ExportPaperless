using ExportPaperless.Domain.Clients;
using ExportPaperless.Domain.Services;
using ExportPaperless.Rest.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExportPaperless.Rest.Controller;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = "ApiKeyScheme")]
public class SavedViewController(
    IExportPaperlessService exportPaperlessService,
    ITokenProvider tokenProvider,
    IStorageService storageService,
    IPaperlessClientFactory paperlessClientFactory)
    : BaseController(tokenProvider)
{
    [HttpGet("export/{id}")]
    public async Task<IActionResult> ExportView([FromRoute] int id, CancellationToken cancellationToken)
    {
        SetClientToken();
        
        var zipBytes = await exportPaperlessService.ExportSavedView(id, cancellationToken);
        return File(zipBytes, "application/zip", GenerateExportFileName(id.ToString()));
    }

    [HttpGet("")]
    public async Task<ActionResult<List<SavedViewDto>>> GetSavedViews(CancellationToken cancellationToken)
    {
        SetClientToken();
        
        var client = paperlessClientFactory.CreateClient();
        var views = await client.GetSavedViews(cancellationToken);

        return views.Select(v => new SavedViewDto { Id = v.Id, Name = v.Name }).ToList();
    }

    [HttpPost("store/{id}")]
    public async Task<ActionResult<string>> StoreSavedViewWithDocuments([FromRoute] int id, CancellationToken cancellationToken)
    {
        SetClientToken();
        
        var zipBytes = await exportPaperlessService.ExportSavedView(id, cancellationToken);
        var fingerprint = await storageService.StoreFile(GenerateExportFileName(id.ToString()), zipBytes, cancellationToken);
        return CreateDownloadActionResult(fingerprint);
    }

    [HttpPost("store/metadata/{id}")]
    public async Task<ActionResult<string>> StoreSavedViewMetadata([FromRoute] int id, CancellationToken cancellationToken)
    {
        SetClientToken();
        
        var excelBytes = await exportPaperlessService.ExportSavedViewMetadata(id, cancellationToken);
        var fingerprint = await storageService.StoreFile(GenerateExportFileName(id.ToString(), ".xlsx"), excelBytes, cancellationToken);
        return CreateDownloadActionResult(fingerprint);
    }
}