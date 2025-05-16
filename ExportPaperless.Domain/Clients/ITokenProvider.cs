namespace ExportPaperless.Domain.Clients;

public interface ITokenProvider
{
    string? CurrentToken { get; set; }
}
