using ExportPaperless.Domain.Services;
using Microsoft.Extensions.Configuration;

namespace ExportPaperless.PaperlessApi.Services;

public class PaperlessConfigurationService(IConfiguration configuration) : IPaperlessConfigurationService
{
    public Uri BaseAddress
    {
        get
        {
            var baseAddress = configuration["API_URL"];
            if (string.IsNullOrEmpty(baseAddress))
            {
                throw new InvalidOperationException(
                    "API_URL variable is not set (appsettings.json), you can also use an environment variable in the format: PE_API_URL=http://localhost:8000");
            }

            return new Uri(baseAddress);
        }
    }

    public string Token
    {
        get
        {
            var token = configuration["API_TOKEN"];
            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException(
                    "API_TOKEN variable is not set (appsettings.json), you can also use an environment variable in the format: PE_API_TOKEN=<API_TOKEN_FROM_PAPERLESS>");
            }

            return token;
        }
    }
}