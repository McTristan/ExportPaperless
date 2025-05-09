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
  - Custom fields (user-defined)
- Generates a well-structured Excel file (via OpenXML SDK)
- Bundles Excel + documents into a ZIP file
- Exposes a single, simple HTTP endpoint:
  - /api/Export/export
- Exposes an OpenAPI compatible Swagger Documentation at /swagger
