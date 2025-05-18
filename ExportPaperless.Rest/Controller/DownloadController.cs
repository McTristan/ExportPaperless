using ExportPaperless.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExportPaperless.Rest.Controller;

[ApiController]
[Route("api/[controller]")]
public class DownloadController(IStorageService storageService) : ControllerBase
{
    [HttpGet("download/{fingerprint}")]
    [AllowAnonymous]
    public async Task<IActionResult> DownloadStoredFile([FromRoute] string fingerprint, CancellationToken cancellationToken)
    {
        var (content, fileName, contentType) = await storageService.GetFile(fingerprint, true, cancellationToken);
        return File(content, contentType, fileName);
    }
}