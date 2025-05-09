using System.Text.Json.Serialization;

namespace ExportPaperless.PaperlessApi.DataContracts;

[Serializable]
public class LookupEntryDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}