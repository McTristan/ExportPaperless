using ExportPaperless.Domain.Clients;

namespace ExportPaperless.PaperlessApi.Clients;

public class TokenProvider: ITokenProvider
{
    public string? CurrentToken { get; set; }
}