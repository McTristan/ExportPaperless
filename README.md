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

Saved Views can be created by a user inside the Paperless UI. When exporting using this api we apply the most common filters from the view and try to export the metadata as accurate as possible.

Authentification with the Paperless NGX Api can be done either on the server side by setting the PAPERLESS__API_TOKEN environment variable or by specifying a request header (x-api-key). Either way you can only export documents and metadata you have access to.

The following additional environment variables need to be setup:

**PAPERLESS__API_URL** <- the URL to the paperless api, i.e. http://localhost:8855/api/ - can be internal or external (i.e. reverse proxied)

**PAPERLESS__PUBLIC_URL** <- the URL of the paperless website, i.e. https://paperless.my.domain

The following additional environment variables are optional but useful:

**EXCEL__DATE_FORMAT** <- how to export date formats to excel, can be a valid date (time) mask like yyyy-MM-dd, dd.MM.yyyy

**EXCEL__STRIP_CURRENCY** <- currency values are stored as strings inside paperless, to get meaningful numbers out of it we need to strip the currency string i.e. EUR or USD from the string and try to parse the value as a double (only EUR and USD supported at the moment)

**EXCEL__NUMBER_FORMAT** <- how to format numbers in Excel, can be a valid number mask like 0.00 (2 decimal places)


There are pre-built Docker-Images for amd64 and arm64:

docker.io/mctristan/paperless-export-api

use the :latest tag for the latest release, the corresponding version tag (i.e. v0.0.1) for a specific version.
