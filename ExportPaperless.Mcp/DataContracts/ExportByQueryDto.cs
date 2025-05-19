using System.Runtime.Serialization;

namespace ExportPaperless.Mcp.DataContracts;

[DataContract]
public class ExportByQueryDto
{
    [DataMember]
    public DateTime? From { get; set; }
    [DataMember]
    public DateTime? To { get; set; }
    [DataMember]
    public List<string>? IncludeTags { get; set; }
    [DataMember]
    public List<string>? ExcludeTags { get; set; }
    [DataMember]
    public List<string>? IncludeDocumentTypes { get; set; }
    [DataMember]
    public List<string>? IncludeCustomFields { get; set; }
    [DataMember]
    public List<string>? IncludeCorrespondents { get; set; }
}