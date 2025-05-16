using System.Net.Http.Headers;
using ExportPaperless.Domain.Clients;
using ExportPaperless.Domain.Services;

namespace ExportPaperless.PaperlessApi.Clients;

public class PaperlessClientFactory(
    IHttpClientFactory httpClientFactory,
    IPaperlessConfigurationService configurationService,
    ITokenProvider tokenProvider)
    : IPaperlessClientFactory
{
    public IPaperlessClient CreateClient(string? overrideToken = null)
    {
        var httpClient = httpClientFactory.CreateClient("Paperless");
        var token = overrideToken ?? tokenProvider.CurrentToken ?? configurationService.Token;
        
        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidOperationException("No API token configured for Paperless API");
        }

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", token);
        return new PaperlessClient(httpClient, configurationService);
    }
}