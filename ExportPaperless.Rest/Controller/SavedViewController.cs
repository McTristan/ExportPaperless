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
        return File(zipBytes, "application/zip", GenerateExportFileName(id));
    }
    
    [HttpPost("store/{id}")]
    public async Task<ActionResult<string>> ExportViewToStorage([FromRoute] int id, CancellationToken cancellationToken)
    {
        SetClientToken();
        
        var zipBytes = await exportPaperlessService.ExportSavedView(id, cancellationToken);
        var fingerprint = await storageService.StoreFile(GenerateExportFileName(id), zipBytes, cancellationToken);
        return CreatedAtAction(nameof(DownloadView), new { fingerprint }, fingerprint);
    }

    [HttpGet("download/{fingerprint}")]
    [AllowAnonymous]
    public async Task<IActionResult> DownloadView([FromRoute] string fingerprint, CancellationToken cancellationToken)
    {
        SetClientToken();
        
        var (content, fileName) = await storageService.GetFile(fingerprint, true, cancellationToken);
        return File(content, "application/zip", fileName);
    }

    [HttpGet("")]
    public async Task<ActionResult<List<SavedViewDto>>> GetSavedViews(CancellationToken cancellationToken)
    {
        SetClientToken();
        
        var client = paperlessClientFactory.CreateClient();
        var views = await client.GetSavedViews(cancellationToken);

        return views.Select(v => new SavedViewDto { Id = v.Id, Name = v.Name }).ToList();

    }
    
    private static string GenerateExportFileName(int id)
    {
        var fileName = $"Export_{id}_{DateTime.Now:yyyyMMddHHmmss}.zip";
        return fileName;
    }

}