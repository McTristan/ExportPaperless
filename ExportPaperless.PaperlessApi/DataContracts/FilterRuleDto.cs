using System.Text.Json.Serialization;
using ExportPaperless.Domain;

namespace ExportPaperless.PaperlessApi.DataContracts;

[Serializable]
public class FilterRuleDto
{
    [JsonPropertyName("rule_type")]
    public RuleType Type { get; set; }
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}