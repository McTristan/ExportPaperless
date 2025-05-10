using System.Text.Json;

namespace ExportPaperless.Domain.Entities;

public class PaperlessDocument(
    int id,
    string title,
    string fileName,
    DateTime created,
    string? correspondent,
    string? documentType,
    string[]? tags,
    string[]? notes,
    Dictionary<string, JsonElement?> customFields,
    Uri url)
{
    public int Id { get; } = id;
    public string Title { get; } = title;
    public DateTime Created { get; } = created;
    public List<string>? Tags { get; } = tags?.ToList();
    public string FileName { get; } = fileName;
    public string? Correspondent { get; } = correspondent;
    public string? DocumentType { get; } = documentType;
    public List<string>? Notes { get; } = notes?.ToList();
    public Dictionary<string, JsonElement?>? CustomFields { get; } = customFields;

    public Uri Url { get; } = url;
}