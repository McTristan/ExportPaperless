using ExportPaperless.Domain.Services;
using Microsoft.Extensions.Configuration;

namespace ExportPaperless.PaperlessApi.Services;

public class PaperlessConfigurationService(IConfiguration configuration) : IPaperlessConfigurationService
{
    private readonly IConfigurationSection _section = configuration.GetSection("PAPERLESS");
    public Uri BaseAddress
    {
        get
        {
            var baseAddress = _section["API_URL"];
            if (string.IsNullOrEmpty(baseAddress))
            {
                throw new InvalidOperationException(
                    "API_URL variable is not set (appsettings.json), you can also use an environment variable in the format: PAPERLESS__API_URL=http://localhost:8000");
            }

            return new Uri(baseAddress);
        }
    }

    public Uri PublicAddress
    {
        get
        {
            var publicAddress = _section["PUBLIC_URL"];
            if (string.IsNullOrEmpty(publicAddress))
            {
                throw new InvalidOperationException("PUBLIC_URL variable is not set (appsettings.json), you can also use an environment variable in the format: PAPERLESS__PUBLIC_URL=https://mypaperless.domain.org");
            }
            
            return new Uri(publicAddress);
        }
    }

    public string Token
    {
        get
        {
            var token = _section["API_TOKEN"];
            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException(
                    "API_TOKEN variable is not set (appsettings.json), you can also use an environment variable in the format: PAPERLESS__API_TOKEN=<API_TOKEN_FROM_PAPERLESS>");
            }

            return token;
        }
    }
}