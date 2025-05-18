namespace ExportPaperless.Storage;

public static class MimeTypeMap
{
    private static readonly Dictionary<string, string> Mappings = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".txt", "text/plain" },
        { ".pdf", "application/pdf" },
        { ".doc", "application/vnd.ms-word" },
        { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        { ".xls", "application/vnd.ms-excel" },
        { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
        { ".png", "image/png" },
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".gif", "image/gif" },
        { ".csv", "text/csv" },
        { ".zip", "application/zip" },
        { ".json", "application/json" },
        { ".xml", "application/xml" },
        { ".html", "text/html" },
        { ".htm", "text/html" }
    };

    public static string GetMimeType(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        
        if (string.IsNullOrEmpty(extension) || !Mappings.TryGetValue(extension, out var mime))
        {
            return "application/octet-stream"; // Standard-Fallback
        }
        
        return mime;
    }
}
