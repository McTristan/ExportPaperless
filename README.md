# 📄 Paperless Export API

Docker images:

https://hub.docker.com/repositories/mctristan

A .NET 10 Web API that integrates with [Paperless-ngx](https://github.com/paperless-ngx/paperless-ngx) to generate Excel exports and download documents as ZIP files — filtered by date, tags (include and exclude) and document type.

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
- can store documents and excel metadata of saved views and return an url to download them later (one time download link)
- provides a first version of a Mcp Server which allows to export saved views by name and/or id

Saved Views can be created by a user inside the Paperless UI. When exporting using this api we apply the most common filters from the view and try to export the metadata as accurate as possible.

Authentification with the Paperless NGX Api can be done either on the server side by setting the PAPERLESS__API_TOKEN environment variable or by specifying a request header (x-api-key). Either way you can only export documents and metadata you have access to.

The following additional environment variables need to be setup:

**PAPERLESS__API_URL** <- the URL to the paperless api, i.e. http://localhost:8855/api/ - can be internal or external (i.e. reverse proxied)

**PAPERLESS__PUBLIC_URL** <- the URL of the paperless website, i.e. https://paperless.my.domain

The following additional environment variables are optional but useful:

**EXCEL__DATE_FORMAT** <- how to export date formats to excel, can be a valid date (time) mask like yyyy-MM-dd, dd.MM.yyyy

**EXCEL__STRIP_CURRENCY** <- (boolean) currency values are stored as strings inside paperless, to get meaningful numbers out of it we need to strip the currency string i.e. EUR or USD from the string and try to parse the value as a double (only EUR and USD supported at the moment)

**EXCEL__NUMBER_FORMAT** <- how to format numbers in Excel, can be a valid number mask like 0.00 (2 decimal places)

**EXCEL__REPLACEMENT_TITLE** <- the name of the title column in the resulting excel file, default is "Title"

**EXCEL__REPLACEMENT_TAGS** <- the name of the tags column in the resulting excel file, default is "Tags"

**EXCEL__REPLACEMENT_CREATED** <- the name of the created date column in the resulting excel file, default is "Created"

**EXCEL__REPLACEMENT_NOTES** <- the name of the notes column in the resulting excel file, default is "Notes"

**EXCEL__REPLACEMENT_DOCUMENT_TYPE** <- the name of the document type column in the resulting excel file, default is "Document Type"

**EXCEL__REPLACEMENT_CORRESPONDENT** <- the name of the correspondent column in the resulting excel file, default is "Correspondent"

**EXCEL__REPLACEMENT_PAGE_COUNT** <- the name of the page count column in the resulting excel file, default is "Page Count"

**EXCEL__REPLACEMENT_FILE_NAME** <- the name of the file name column in the resulting excel file, default is "Filename"

**EXCEL__REPLACEMENT_URL** <- the name of the URL column in the resulting excel file, default is "URL"

**STORAGE__DATA_PATH** <- where to store files for one-time downloads (see McpServer section)

There are pre-built Docker-Images for amd64 and arm64:

docker.io/mctristan/paperless-export-api

use the :latest tag for the latest release, the corresponding version tag (i.e. v0.0.1) for a specific version.

## MCP Server

#### Note: 
The Model Context Protocol support is an early trial as the protocol is rather new and the tool supported rather limited.
There is no docker support for the McpServer part yet so you have to have a dotnet runtime installed and you would need to check out the source and configure your favorite Ai tool to use the server.

A sample Claude configuration could look like this:

in `claude_desktop_config.json`
```
{
  "mcpServers": {
    "paperless-saved-view-exporter": {
      "command": "/Users/myusername/.dotnet/dotnet",
      "args": [
        "run",
        "--project",
        "/Users/myusername/Desktop/Projects/ExportPaperless/ExportPaperless.McpServer",
        "--no-build"
      ],      
      "env": {        
        "MCP__EXPORT_PAPERLESS_API_URL": "http://localhost:5288/"
      }
    }
  }
}
```

Of course the ExportPaperless API needs to be running (i.e. as docker image or via dotnet run) as long as you want to use the McpServer tool.

Unfortunately Claude AI is not very helpful with error handling at the moment, you will most of the time only get an "invocation error" - that's it.
Make sure your environment variables are correct (both in your paperless export api and the claude configuration file).

Make also sure you allow the access to the various tools (i.e.: always allow).

A sample query for Claude would then be:

```
Can you help me export the documents of my saved view Invoices 2024 from my paperless instance?
```

or

```
Can you export invoices from paperless for me?
```

or

```
Can you export the saved view Invoices 2024 as excel for me?
```

I guess you get the idea. What you will get in return (if everything works out) is a URL of .zip or .xlsx file which you can download (one time download).

### Using the McpServer docker image

```
{
  "mcpServers": {
    "paperless-saved-view-exporter": {
      "command": "docker",
      "args": [
        "run",
        "-i",
        "--rm",
        "mctristan/paperless-export-mcp-server:latest",
        "-e MCP__EXPORT_PAPERLESS_API_URL=http://localhost:5288/"
      ]
    }
  }
}
```

## MCP Server SSE
When started the SSE server should be listening on port 5225, the url to connect to is http://localhost:5225/sse or the corresponding hostname or IP address of the machine.

