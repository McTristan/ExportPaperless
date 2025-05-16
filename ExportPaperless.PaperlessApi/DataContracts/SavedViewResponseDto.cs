using System.Text.Json.Serialization;

namespace ExportPaperless.PaperlessApi.DataContracts;

[Serializable]
public class SavedViewResponseDto
{
    [JsonPropertyName("next")]
    public string Next { get; set; } = string.Empty;
    [JsonPropertyName("results")]
    public List<SavedViewDto> Results { get; set; } = new();
}