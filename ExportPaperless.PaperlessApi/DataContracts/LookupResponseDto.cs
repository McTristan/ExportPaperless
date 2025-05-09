using System.Text.Json.Serialization;

namespace ExportPaperless.PaperlessApi.DataContracts;

[Serializable]
public class LookupResponseDto
{
    [JsonPropertyName("next")]
    public string Next { get; set; } = string.Empty;
    [JsonPropertyName("results")]
    public List<LookupEntryDto> Results { get; set; } = new();
}