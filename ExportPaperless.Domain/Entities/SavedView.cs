namespace ExportPaperless.Domain.Entities;

public class SavedView(int viewId, string name, List<string> displayFields, List<FilterRule> filterRules, Dictionary<int, string> customFields)
{
    public int Id { get; } = viewId;
    public string Name { get; } = name;
    public List<string> DisplayFields { get; } = displayFields;
    public List<FilterRule> FilterRules { get; } = filterRules;
    public Dictionary<int, string> CustomFields { get; } = customFields;
}