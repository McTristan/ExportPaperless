using System.Text.Json;
using System.Text.Json.Serialization;

namespace ExportPaperless.Domain;

public static class JsonOptions
{
    public static JsonSerializerOptions Defaults()
    {
        return new JsonSerializerOptions { 
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };
    }

}
