using System.Runtime.Serialization;

namespace ExportPaperless.Rest.DataContracts;

[DataContract]
public class SavedViewDto
{
    [DataMember]
    public int Id { get; set; }
    
    [DataMember]
    public string Name { get; set; } = string.Empty;
}