namespace ExportPaperless.Domain.Services;

public interface IPaperlessConfigurationService
{
    public Uri BaseAddress { get; }
    public string Token { get; }
}