using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ExportPaperless.Domain;
using ExportPaperless.Domain.Clients;
using ExportPaperless.Domain.Entities;
using ExportPaperless.Domain.Services;
using ExportPaperless.PaperlessApi.DataContracts;
using Microsoft.AspNetCore.Http;

namespace ExportPaperless.PaperlessApi.Clients;

public class PaperlessClient : IPaperlessClient
{
    private readonly HttpClient _httpClient;
    private readonly IPaperlessConfigurationService _configurationService;

    public PaperlessClient(IHttpContextAccessor contextAccessor, IHttpClientFactory httpClientFactory, IPaperlessConfigurationService configurationService)
    {
        _configurationService = configurationService;
        _httpClient = httpClientFactory.CreateClient("Paperless");
        var token = contextAccessor.HttpContext?.Request.Headers["x-api-key"];
        if (string.IsNullOrEmpty(token))
        {
            token = configurationService.Token;
        }
        
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", token);
    }

    public async Task<List<PaperlessDocument>> GetDocuments(DateTime from, DateTime to, List<string> includeTags, 
        List<string> excludeTags, List<string> includeDocumentTypes, List<string> includeCustomFields, List<string> includeCorrespondents, 
        CancellationToken cancellationToken)
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
        
        var includeCustomFieldIds = await GetIdsFromNames(includeCustomFields, "custom_fields");
        var includeCustomFieldQuery = string.Join(",", includeCustomFieldIds.Select(id => id.ToString()));
        if (!string.IsNullOrEmpty(includeCustomFieldQuery))
        {
            baseUrl += $"&custom_fields__id__all={includeCustomFieldQuery}";
        }

        var includeCorrespondentIds = await GetIdsFromNames(includeCorrespondents, "correspondents");
        var includeCorrespondentQuery = string.Join(",", includeCorrespondentIds.Select(id => id.ToString()));
        if (!string.IsNullOrEmpty(includeCorrespondentQuery))
        {
            baseUrl += $"&correspondent__id__in={includeCorrespondentQuery}";
        }
        var fullDocs = await FilterDocuments(baseUrl, cancellationToken);
        return fullDocs.OrderBy(f => f.Created).ToList();
    }

    private async Task<Dictionary<int, string>> GetCustomFieldNamesForSavedView(SavedViewDto viewDto, CancellationToken cancellationToken)
    {
        var customFields = await GetLookup("custom_fields", cancellationToken);
        var fieldIdToNames = new Dictionary<int, string>();

        foreach (var filterRule in viewDto.FilterRules.Where(fr => fr.Type is RuleType.HasCustomField or RuleType.HasTheseCustomFields))
        {
            var customFieldId = int.Parse(filterRule.Value);
            fieldIdToNames[customFieldId] = customFields[customFieldId];
        }

        foreach (var displayField in viewDto.DisplayFields.Where(df => df.StartsWith("custom_field_", StringComparison.OrdinalIgnoreCase))) 
        {
            var customFieldId = int.Parse(displayField.Replace("custom_field_", ""));
            fieldIdToNames[customFieldId] = customFields[customFieldId];
        }

        return fieldIdToNames;
    }

    private async Task<List<PaperlessDocument>> FilterDocuments(string filterUrl, CancellationToken cancellationToken)
    {
        var fullDocs = new List<PaperlessDocument>();
        var nextUrl = filterUrl;

        // Lookup tables
        var correspondents = await GetLookup("correspondents", cancellationToken);
        var docTypes = await GetLookup("document_types", cancellationToken);
        var tags = await GetLookup("tags", cancellationToken);
        var customFields = await GetLookup("custom_fields", cancellationToken);
        
        while (!string.IsNullOrEmpty(nextUrl))
        {
            var listResponse = await _httpClient.GetFromJsonAsync<PaperlessDocumentListResponseDto>(nextUrl, cancellationToken: cancellationToken);
            if (listResponse?.Results == null) break;

            foreach (var doc in listResponse.Results)
            {
                var correspondent = doc.CorrespondentId.HasValue ? correspondents[doc.CorrespondentId.Value] : "";
                var documentType = doc.DocumentTypeId.HasValue ? docTypes[doc.DocumentTypeId.Value] : "";
                var namedTags = doc.Tags?.Select(tag => tags.FirstOrDefault(f => f.Key == tag).Value).ToList();
                var namedCustomFields = new Dictionary<string, JsonElement?>();
                if (doc.CustomFields != null)
                {
                    foreach (var field in doc.CustomFields)
                    {
                        var fieldName = customFields.FirstOrDefault(f => f.Key == field.Field).Value;
                        namedCustomFields[fieldName] = field.Value;
                    }
                }

                var url = new Uri(_configurationService.PublicAddress, $"documents/{doc.Id}/details");
                var paperlessDocument = new PaperlessDocument(doc.Id, doc.Title,
                    string.IsNullOrEmpty(doc.FileName) ? doc.OriginalFileName : doc.FileName, doc.Created,
                    correspondent, documentType, namedTags?.ToArray(), doc.Notes?.Select(n => n.Note).ToArray(),
                    namedCustomFields, doc.PageCount, url);
               
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

    public async Task<List<PaperlessDocument>> GetDocumentsFromView(int viewId, CancellationToken cancellationToken)
    {
        var view = await GetView(viewId, cancellationToken);

        var baseUrl = $"documents/";

        var filterRulesGroup = view!.FilterRules.GroupBy(fr => fr.Type);
        foreach (var filterRuleGroup in filterRulesGroup)
        {
            switch (filterRuleGroup.Key)
            {
                case RuleType.HasTag:
                    var includedTagsQuery = string.Join(",",
                        filterRuleGroup.Select(fr => int.Parse(fr.Value)).ToList().Select(id => id.ToString()));
                    if (!string.IsNullOrEmpty(includedTagsQuery))
                    {
                        baseUrl = QueryPrefix(baseUrl);
                        baseUrl += $"tags__id__all={includedTagsQuery}";
                    }
                    break;
                case RuleType.HasNotTag:
                    var excludedTagsQuery = string.Join(",",
                        filterRuleGroup.Select(fr => int.Parse(fr.Value)).ToList().Select(id => id.ToString()));
                    if (!string.IsNullOrEmpty(excludedTagsQuery))
                    {
                        baseUrl = QueryPrefix(baseUrl);
                        baseUrl += $"tags__id__none={excludedTagsQuery}";
                    }
                    break;
                case RuleType.DocumentTypeIs:
                    var documentTypesQuery = string.Join(",",
                        filterRuleGroup.Select(fr => int.Parse(fr.Value)).ToList().Select(id => id.ToString()));
                    if (!string.IsNullOrEmpty(documentTypesQuery))
                    {
                        baseUrl = QueryPrefix(baseUrl);
                        baseUrl += $"document_type__id__in={documentTypesQuery}";
                    }
                    break;
                case RuleType.CreatedFrom:
                    baseUrl = QueryPrefix(baseUrl);
                    baseUrl += $"created__date__gte={filterRuleGroup.First().Value}";
                    break;
                case RuleType.CreatedTo:
                    baseUrl = QueryPrefix(baseUrl);
                    baseUrl += $"created__date__lte={filterRuleGroup.First().Value}";
                    break;
            }
        }

        if (!string.IsNullOrEmpty(view.Sort))
        {
            baseUrl = QueryPrefix(baseUrl);
            var sortDirection = view.SortDescending ? "-" : "";
            baseUrl += $"ordering={sortDirection}{view.Sort}";
        }
        
        return await FilterDocuments(baseUrl, cancellationToken);
    }

    private async Task<SavedViewDto?> GetView(int viewId, CancellationToken cancellationToken)
    {
        var view = await _httpClient.GetFromJsonAsync<SavedViewDto?>($"saved_views/{viewId}/", cancellationToken);
        if (view == null)
        {
            throw new InvalidOperationException($"View with id {viewId} not found");
        }

        return view;
    }

    public async Task<SavedView> GetSavedView(int viewId, CancellationToken cancellationToken)
    {
        var viewDto = await GetView(viewId, cancellationToken);
        var filterRules = viewDto!.FilterRules.Select(ruleDto => new FilterRule(ruleDto.Type, ruleDto.Value)).ToList();
        var customFieldNamesMapping = await GetCustomFieldNamesForSavedView(viewDto, cancellationToken);
        
        return new SavedView(viewDto.Id, viewDto.Name, viewDto.DisplayFields, filterRules, customFieldNamesMapping);
    }
    
    private static string QueryPrefix(string baseUrl)
    {
        if (baseUrl.Contains('?'))
        {
            baseUrl += "&";
        }
        else
        {
            baseUrl += "?";
        }

        return baseUrl;
    }

    private async Task<Dictionary<int, string>> GetLookup(string endpoint, CancellationToken cancellationToken = default)
    {
        var results = new List<LookupEntryDto>();
        var nextUrl = $"{endpoint}/";

        while (!string.IsNullOrEmpty(nextUrl))
        {
            var response = await _httpClient.GetFromJsonAsync<LookupResponseDto>(nextUrl, cancellationToken: cancellationToken);
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