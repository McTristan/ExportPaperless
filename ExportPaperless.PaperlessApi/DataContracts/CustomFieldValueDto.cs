namespace ExportPaperless.PaperlessApi.DataContracts;

using System.Text.Json.Serialization;

[Serializable]
public class CustomFieldValueDto
{
    [JsonPropertyName("field")]
    public int Field { get; set; }
    
    [JsonPropertyName("value")]
    public object Value { get; set; } = string.Empty;
    
    [JsonIgnore]
    public string? Name { get; set; }
}