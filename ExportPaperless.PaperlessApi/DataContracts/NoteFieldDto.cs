namespace ExportPaperless.PaperlessApi.DataContracts;

using System.Text.Json.Serialization;

[Serializable]
public class NoteFieldDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("note")]
    public string Note { get; set; } = string.Empty;
}