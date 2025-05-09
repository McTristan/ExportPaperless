using System.Net.Http.Json;
using ExportPaperless.Domain.Clients;
using ExportPaperless.Domain.Entities;
using ExportPaperless.PaperlessApi.DataContracts;

namespace ExportPaperless.PaperlessApi.Clients;

public class PaperlessClient(IHttpClientFactory httpClientFactory) : IPaperlessClient
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("Paperless");

    public async Task<List<PaperlessDocument>> GetDocuments(DateTime from, DateTime to, List<string> includeTags, 
        List<string> excludeTags, List<string> includeDocumentTypes, List<string> fields, CancellationToken cancellationToken)
    {
        var baseUrl = $"documents/?created__gte={from:yyyy-MM-dd}&created__lte={to:yyyy-MM-dd}";
        var includeTagIds = await GetIdsFromNames(includeTags, "tags");
        var includedTagsQuery = string.Join(",", includeTagIds.Select(id => id.ToString()));
        if (!string.IsNullOrEmpty(includedTagsQuery))
        {
            baseUrl += $"&tags__id__all={includedTagsQuery}";
        }
        var excludeTagIds = await GetIdsFromNames(excludeTags, "tags");
        var excludedTagsQuery = string.Join(",", excludeTagIds.Select(id => id.ToString()));
        if (!string.IsNullOrEmpty(excludedTagsQuery))
        {
            baseUrl += $"&tags__id__none={excludedTagsQuery}";
        }
        var includeDocumentTypeIds = await GetIdsFromNames(includeDocumentTypes, "document_types");
        var includedDocumentTypesQuery = string.Join(",", includeDocumentTypeIds.Select(id => id.ToString()));
        if (!string.IsNullOrEmpty(includedDocumentTypesQuery))
        {
            baseUrl += $"&document_type__id__in={includedDocumentTypesQuery}";
        }
        var fullDocs = new List<PaperlessDocument>();
        var nextUrl = baseUrl;

        // Lookup tables
        var correspondents = await GetLookup("correspondents");
        var docTypes = await GetLookup("document_types");
        var tags = await GetLookup("tags");
        var customFields = await GetLookup("custom_fields");
        
        while (!string.IsNullOrEmpty(nextUrl))
        {
            var listResponse = await _httpClient.GetFromJsonAsync<PaperlessDocumentListResponseDto>(nextUrl, cancellationToken: cancellationToken);
            if (listResponse?.Results == null) break;

            foreach (var doc in listResponse.Results)
            {
                var correspondent = doc.CorrespondentId.HasValue ? correspondents[doc.CorrespondentId.Value] : "";
                var documentType = doc.DocumentTypeId.HasValue ? docTypes[doc.DocumentTypeId.Value] : "";
                var namedTags = doc.Tags?.Select(tag => tags.FirstOrDefault(f => f.Key == tag).Value).ToList();
                var namedCustomFields = new Dictionary<string, object>();
                if (doc.CustomFields != null)
                {
                    foreach (var field in doc.CustomFields)
                    {
                        var fieldName = customFields.FirstOrDefault(f => f.Key == field.Field).Value;
                        namedCustomFields[fieldName] = field.Value;
                    }
                }

                var paperlessDocument = new PaperlessDocument(doc.Id, doc.Title,
                    string.IsNullOrEmpty(doc.FileName) ? doc.OriginalFileName : doc.FileName, doc.Created,
                    correspondent, documentType, namedTags?.ToArray(), doc.Notes?.Select(n => n.Note).ToArray(), namedCustomFields);
               
                fullDocs.Add(paperlessDocument);
            }

            nextUrl = listResponse.Next;
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (nextUrl != null && nextUrl.StartsWith('/'))
            {
                nextUrl = nextUrl.TrimStart('/');
            }
        }
        return fullDocs;
    }
    
    private async Task<Dictionary<int, string>> GetLookup(string endpoint)
    {
        var results = new List<LookupEntryDto>();
        var nextUrl = $"{endpoint}/";

        while (!string.IsNullOrEmpty(nextUrl))
        {
            var response = await _httpClient.GetFromJsonAsync<LookupResponseDto>(nextUrl);
            if (response?.Results != null)
            {
                results.AddRange(response.Results);
                nextUrl = response.Next;

                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                if (nextUrl != null && nextUrl.StartsWith("/"))
                {
                    nextUrl = nextUrl.TrimStart('/');
                }
            }
            else break;
        }

        return results.ToDictionary(r => r.Id, r => r.Name);
    }

    private async Task<List<int>> GetIdsFromNames(List<string> names, string endpoint)
    {
        var types = await GetLookup(endpoint);
        return types.Where(t => names.Contains(t.Value, StringComparer.OrdinalIgnoreCase))
            .Select(t => t.Key)
            .ToList();
    }
}