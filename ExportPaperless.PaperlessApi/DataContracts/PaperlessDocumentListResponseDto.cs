using System.Text.Json.Serialization;

namespace ExportPaperless.PaperlessApi.DataContracts;

[Serializable]
public class PaperlessDocumentListResponseDto
{
    [JsonPropertyName("count")]
    public int Count { get; set; }
    
    [JsonPropertyName("next")]
    public string Next { get; set; } = string.Empty;
    
    [JsonPropertyName("previous")]
    public string Previous { get; set; } = string.Empty;
    
    [JsonPropertyName("results")]
    public List<PaperlessDocumentDto> Results { get; set; } = new();
}