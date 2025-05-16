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

}