namespace ExportPaperless.Domain.Services;

public interface IPaperlessConfigurationService
{
    public Uri BaseAddress { get; }
    public Uri PublicAddress { get; }
    public string Token { get; }
}