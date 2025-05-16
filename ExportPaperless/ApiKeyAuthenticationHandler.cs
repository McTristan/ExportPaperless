using System.Security.Claims;
using System.Text.Encodings.Web;
using ExportPaperless.Domain.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IPaperlessConfigurationService _configurationService;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IPaperlessConfigurationService configurationService)
        : base(options, logger, encoder)
    {
        _configurationService = configurationService;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var configuredKey = _configurationService.Token;
        if (string.IsNullOrEmpty(configuredKey) && Request.Headers.TryGetValue("x-api-Key", out _))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing API Key, use x-api-Key header (on client side) or PAPERLESS__API_TOKEN environment variable (on server side)"));
        }
        
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "apikey") };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}