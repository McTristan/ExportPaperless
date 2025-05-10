# 📄 Paperless Export API

A .NET 8 Web API that integrates with [Paperless-ngx](https://github.com/paperless-ngx/paperless-ngx) to generate Excel exports and download documents as ZIP files — filtered by date, tags (include and exclude) and document type.

## ✨ Features

- Connects to a running Paperless-ngx instance via its REST API
- Filters documents by:
  - Date range (`from`, `to`)
  - Tags (by tag slug, i.e. use Invoice etc. instead of the Id)
  - document type (as a string)
- Resolves related entities like:
  - Correspondents
  - Document types
  - Custom fields (user-defined - specify them in the query if you want to have them in the resulting excel file)
- Generates a well-structured Excel file (via OpenXML SDK)
- Bundles Excel + documents into a ZIP file
- Exposes a a couple of simple HTTP endpoints:
  - **/api/Export**       -> uses multiple filters via query parameters (see swagger) and returns a compress zip file containing the documents and a metadata.xlsx file
  - **/api/Export/view**  -> exports a saved view (user defined) and returns the same zip. Query parameters, ordering and so on is taken from the configured settings of the corresponding view
- Exposes an OpenAPI compatible Swagger Documentation at /swagger
