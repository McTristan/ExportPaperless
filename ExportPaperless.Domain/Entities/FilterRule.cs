namespace ExportPaperless.Domain.Entities;

public class FilterRule(RuleType type, string value)
{
    public RuleType Type { get; } = type;
    public string Value { get; } = value;
}