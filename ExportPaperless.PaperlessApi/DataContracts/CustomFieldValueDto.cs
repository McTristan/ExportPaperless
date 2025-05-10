using System.Text.Json;

namespace ExportPaperless.PaperlessApi.DataContracts;

using System.Text.Json.Serialization;

[Serializable]
public class CustomFieldValueDto
{
    [JsonPropertyName("field")]
    public int Field { get; set; }
    
    [JsonPropertyName("value")]
    public JsonElement? Value { get; set; }
    
    [JsonIgnore]
    public string? Name { get; set; }
}