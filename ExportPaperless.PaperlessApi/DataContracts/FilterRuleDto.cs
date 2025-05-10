using System.Text.Json.Serialization;

namespace ExportPaperless.PaperlessApi.DataContracts;

[Serializable]
public class FilterRuleDto
{
    [JsonPropertyName("rule_type")]
    public RuleType Type { get; set; }
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}

public enum RuleType
{
    TitleContains = 0,
    ContentContains = 1,
    ASNIs = 2,
    CorrespondentIs = 3,
    DocumentTypeIs = 4,
    IsInInbox = 5,
    HasTag = 6,
    HasAnyTag = 7,
    IssuedBefore = 8,
    IssuedAfter = 9,
    IssuedInYear = 10,
    IssuedInMonth = 11,
    IssuedOnDay = 12,
    AddedBefore = 13,
    AddedAfter = 14,
    ModifiedBefore = 15,
    ModifiedAfter = 16,
    HasNotTag = 17,
    HasNoAsn = 18,
    TitleOrContentContains = 19,
    FullTextSearch = 20,
    SimilarDocuments = 21,
    HasTagsIn = 22,
    AsnGreaterThan = 23,
    AsnLessThan = 24,
    StoragePathIs = 25,
    HasCorrespondentIn = 26,
    HasNoCorrespondentIn = 27,
    HasDocumentTypeIn = 28,
    HasNoDocumentTypeIn = 29,
    HasStoragePathIn = 30,
    HasNoStoragePathIn = 31,
    OwnerIs = 32,
    HasOwnerIn = 33,
    HasNoOwner = 34,
    HasNoOwnerIn = 35,
    HasCustomField = 36,
    IsSharedByMe = 37,
    HasCustomFields = 38,
    HasTheseCustomFields = 39,
    HasNotTheseCustomFields = 40,
    HasNotThisCustomField = 41,
    HasCustomFieldQuery = 42,
    CreatedTo = 43,
    CreatedFrom = 44,
    AddedTo = 45,
    AddedFrom = 46,
    MimeTypeIs = 47 
}