namespace ExportPaperless.Domain.Clients;

public interface IPaperlessClientFactory
{
    IPaperlessClient CreateClient(string? overrideToken = null);
}
