using System.Text.Json.Serialization;

namespace ExportPaperless.PaperlessApi.DataContracts;

[Serializable]
public class SavedViewDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("sort_field")]
    public string Sort { get; set; } = string.Empty;
    [JsonPropertyName("sort_reverse")]
    public bool SortDescending { get; set; }
    [JsonPropertyName("filter_rules")]
    public List<FilterRuleDto> FilterRules { get; set; } = new();
    [JsonPropertyName("display_fields")]
    public List<string> DisplayFields { get; set; } = new();
}
