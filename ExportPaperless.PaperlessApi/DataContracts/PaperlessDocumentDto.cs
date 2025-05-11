using System.Text.Json.Serialization;

namespace ExportPaperless.PaperlessApi.DataContracts;

[Serializable]
public class PaperlessDocumentDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName( "title")]
    public string Title { get; set; } = string.Empty;
    [JsonPropertyName("created")]
    public DateTime Created { get; set; }
    [JsonPropertyName("tags")]
    public List<int>? Tags { get; set; } = new();
    [JsonPropertyName("archived_file_name")]
    public string FileName { get; set; } = string.Empty;
    
    [JsonPropertyName("original_file_name")]
    public string OriginalFileName { get; set; } = string.Empty;
    
    [JsonPropertyName("correspondent")]
    public int? CorrespondentId { get; set; }
    [JsonPropertyName("document_type")]
    public int? DocumentTypeId { get; set; }
    [JsonPropertyName("notes")]
    public List<NoteFieldDto>? Notes { get; set; } = [];
    [JsonPropertyName("custom_fields")]
    public List<CustomFieldValueDto>? CustomFields { get; set; } = new();
    
    [JsonPropertyName("page_count")]
    public int PageCount { get; set; }
}