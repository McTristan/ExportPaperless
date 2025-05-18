using ExportPaperless.Domain.Clients;
using Microsoft.AspNetCore.Mvc;

namespace ExportPaperless.Rest.Controller;

public class BaseController(ITokenProvider tokenProvider) : ControllerBase
{
    protected void SetClientToken()
    {
        var apiKey = Request.Headers["x-api-key"].FirstOrDefault();
        tokenProvider.CurrentToken = apiKey;
    }
    
    private protected string GenerateExportFileName(string prefix, string extension = ".zip")
    {
        var fileName = $"Export_{prefix}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
        return fileName;
    }
    
    protected CreatedAtActionResult CreateDownloadActionResult(string fingerprint)
    {
        return CreatedAtAction(actionName: nameof(DownloadController.DownloadStoredFile), controllerName: "Download", new { fingerprint }, fingerprint);
    }

}