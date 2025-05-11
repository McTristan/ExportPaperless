using System.Globalization;
using System.Text.Json;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using ExportPaperless.Domain.Entities;
using ExportPaperless.Domain.Services;

namespace ExportPaperless.Services;

public class ExcelExportService(IExcelExportConfigurationService configurationService) : IExcelExportService
{
    private const uint CellStyleDate = 1;
    private const uint CellStyleNumber = 2;
    private const uint CellStyleUrl = 3;

    public MemoryStream GenerateExcel(List<PaperlessDocument> documents, List<string> customFields)
    {
        var headerRow = new Row();
        headerRow.Append(
            new Cell { CellValue = new CellValue("Title"), DataType = CellValues.String },
            new Cell { CellValue = new CellValue("Date"), DataType = CellValues.String },
            new Cell { CellValue = new CellValue("Tags"), DataType = CellValues.String },
            new Cell { CellValue = new CellValue("Correspondent"), DataType = CellValues.String },
            new Cell { CellValue = new CellValue("Notes"), DataType = CellValues.String },
            new Cell { CellValue = new CellValue("Document Type"), DataType = CellValues.String },
            new Cell { CellValue = new CellValue("Filename"), DataType = CellValues.String },
            new Cell { CellValue = new CellValue("URL"), DataType = CellValues.String }
        );
        foreach (var fieldName in customFields)
        {
            headerRow.Append(new Cell { CellValue = new CellValue(fieldName), DataType = CellValues.String });
        }

        var rows = new List<Row>();
        foreach (var doc in documents)
        {
            var row = new Row();
            row.Append(new Cell { CellValue = new CellValue(doc.Title), DataType = CellValues.String });
            var createdDateCell = new Cell();
            FormatCellAsDate(doc.Created, createdDateCell);
            row.Append(createdDateCell);

            row.Append(
                new Cell
                {
                    CellValue = new CellValue(string.Join(", ", doc.Tags ?? [])),
                    DataType = CellValues.String
                },
                new Cell { CellValue = new CellValue(doc.Correspondent ?? ""), DataType = CellValues.String },
                new Cell
                {
                    CellValue = new CellValue(string.Join(", ", doc.Notes ?? [])),
                    DataType = CellValues.String
                },
                new Cell { CellValue = new CellValue(doc.DocumentType ?? ""), DataType = CellValues.String },
                new Cell { CellValue = new CellValue(doc.FileName), DataType = CellValues.String });

            var urlCell = new Cell();
            FormatCellAsHyperlink(doc.Url.ToString(), urlCell);
            row.Append(urlCell);

            foreach (var fieldName in customFields)
            {
                CreateCellFromCustomFieldValue(doc, row, fieldName);
            }
            
            rows.Add(row);
        }

        return GenerateExcel("Documents", headerRow, rows);
    }

    private void CreateCellFromCustomFieldValue(PaperlessDocument doc, Row row, string fieldName)
    {
        JsonElement? value = null;
        doc.CustomFields?.TryGetValue(fieldName, out value);

        if (value == null)
        {
            row.Append(new Cell());
            return;
        }

        var cell = new Cell();

        switch (value.Value.ValueKind)
        {
            case JsonValueKind.String:
                var strValue = value.Value.GetString();
                if (strValue == null)
                {
                    break;
                }

                // first try to parse as date
                if (DateTime.TryParse(strValue, CultureInfo.InvariantCulture, DateTimeStyles.None,
                        out var dateValue))
                {
                    FormatCellAsDate(dateValue, cell);
                    break;
                }

                if (configurationService.StripCurrency && IsCurrencyString(strValue))
                {
                    var cleanValue = new string(strValue
                        .Where(c => char.IsDigit(c) || c == '.' || c == ',' || c == '-').ToArray());

                    if (double.TryParse(cleanValue, NumberStyles.Any, CultureInfo.InvariantCulture,
                            out var numericValue))
                    {
                        FormatCellAsNumber(numericValue, cell);
                        ;
                        break;
                    }
                }

                cell.CellValue = new CellValue(value.ToString()!);
                cell.DataType = CellValues.String;
                break;

            case JsonValueKind.Number:
                if (value.Value.TryGetDouble(out var doubleValue))
                {
                    FormatCellAsNumber(doubleValue, cell);
                }

                break;

            default:
                cell.CellValue = new CellValue(value.Value.ToString()!);
                cell.DataType = CellValues.String;
                break;
        }

        row.Append(cell);
    }

    public MemoryStream GenerateExcel(List<PaperlessDocument> documents, SavedView view)
    {
        var headerRow = new Row();
        foreach (var displayField in view.DisplayFields)
        {
            if (displayField.StartsWith("custom_field_", StringComparison.OrdinalIgnoreCase))
            {
                var customFieldId = int.Parse(displayField.Substring("custom_field_".Length));
                var customFieldName = view.CustomFields[customFieldId];
                headerRow.Append(new Cell { CellValue = new CellValue(customFieldName), DataType = CellValues.String });
                continue;
            }

            if (displayField.StartsWith("tag", StringComparison.OrdinalIgnoreCase))
            {
                headerRow.Append(new Cell { CellValue = new CellValue("Tags"), DataType = CellValues.String });
                continue;
            }

            switch (displayField.ToUpperInvariant())
            {
                case "TITLE":
                    headerRow.Append(new Cell { CellValue = new CellValue("Title"), DataType = CellValues.String });
                    break;
                case "CREATED":
                    headerRow.Append(new Cell { CellValue = new CellValue("Created"), DataType = CellValues.String });
                    break;
                case "NOTE":
                    headerRow.Append(new Cell { CellValue = new CellValue("Notes"), DataType = CellValues.String });
                    break;
                case "DOCUMENTTYPE":
                    headerRow.Append(new Cell { CellValue = new CellValue("Document Type"), DataType = CellValues.String });
                    break;
                case "CORRESPONDENT":
                    headerRow.Append(new Cell { CellValue = new CellValue("Correspondent"), DataType = CellValues.String });
                    break;
                case "PAGECOUNT":
                    headerRow.Append(new Cell { CellValue = new CellValue("Page Count"), DataType = CellValues.String });
                    break;
            }
        }

        headerRow.Append(
            new Cell { CellValue = new CellValue("Filename"), DataType = CellValues.String },
            new Cell { CellValue = new CellValue("URL"), DataType = CellValues.String }
        );
        
        var rows = new List<Row>();
        foreach (var doc in documents)
        {
            var row = new Row();

            foreach (var displayField in view.DisplayFields)
            {
                if (displayField.StartsWith("custom_field_", StringComparison.OrdinalIgnoreCase))
                {
                    var customFieldId = int.Parse(displayField.Substring("custom_field_".Length));
                    var customFieldName = view.CustomFields[customFieldId];
                    CreateCellFromCustomFieldValue(doc, row, customFieldName);
                    continue;
                }

                if (displayField.StartsWith("tag", StringComparison.OrdinalIgnoreCase))
                {
                    row.Append(new Cell { CellValue = new CellValue(string.Join(", ", doc.Tags ?? [])), DataType = CellValues.String });
                    continue;
                }
                
                switch (displayField.ToUpperInvariant())
                {
                    case "TITLE":
                        row.Append(new Cell { CellValue = new CellValue(doc.Title), DataType = CellValues.String });
                        break;
                    case "CREATED":
                        var createdDateCell = new Cell();
                        FormatCellAsDate(doc.Created, createdDateCell);
                        row.Append(createdDateCell);
                        break;
                    case "NOTE":
                        row.Append(new Cell
                        {
                            CellValue = new CellValue(string.Join(", ", doc.Notes ?? [])), DataType = CellValues.String
                        });
                        break;
                    case "DOCUMENTTYPE":
                        row.Append(new Cell
                            { CellValue = new CellValue(doc.DocumentType ?? ""), DataType = CellValues.String });
                        break;
                    case "CORRESPONDENT":
                        row.Append(new Cell { CellValue = new CellValue(doc.Correspondent ?? ""), DataType = CellValues.String });
                        break;
                    case "PAGECOUNT":
                        row.Append(new Cell { CellValue = new CellValue(doc.PageCount), DataType = CellValues.Number });
                        break;
                }
            }
            
            row.Append(new Cell { CellValue = new CellValue(doc.FileName), DataType = CellValues.String });
            var urlCell = new Cell();
            FormatCellAsHyperlink(doc.Url.ToString(), urlCell);
            row.Append(urlCell);
            
            rows.Add(row);
        }
        
        return GenerateExcel(view.Name, headerRow, rows);
    }

    private MemoryStream GenerateExcel(string documentName, Row headerRow, List<Row> rows)
    {
        var stream = new MemoryStream();
        using (var spreadsheet = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook, true))
        {
            var workbookPart = spreadsheet.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            AddStyles(workbookPart);

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            var sheets = workbookPart.Workbook.AppendChild(new Sheets());
            var sheet = new Sheet
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = documentName
            };
            sheets.Append(sheet);

            var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

            // Header row
            sheetData?.Append(headerRow);

            // Data rows
            foreach (var row in rows)
            {
                sheetData?.Append(row);
            }

            workbookPart.Workbook.Save();
        }

        stream.Position = 0;
        return stream;
    }

    private static void FormatCellAsDate(DateTime dateValue, Cell cell)
    {
        var excelDate = dateValue.ToOADate();
        cell.CellValue = new CellValue(excelDate);
        cell.DataType = CellValues.Number;
        // set the custom date format
        cell.StyleIndex = CellStyleDate;
    }

    private static void FormatCellAsHyperlink(string url, Cell cell)
    {
        cell.CellValue = new CellValue(url);
        cell.DataType = CellValues.String;
        cell.CellFormula = new CellFormula($"HYPERLINK(\"{url}\",\"{url}\")");
        cell.StyleIndex = CellStyleUrl;
    }

    private static void FormatCellAsNumber(double numberValue, Cell cell)
    {
        cell.CellValue = new CellValue(numberValue.ToString("0.##"));
        cell.DataType = CellValues.Number;
        // set the custom number format
        cell.StyleIndex = CellStyleNumber;
    }

    private void AddStyles(WorkbookPart workbookPart)
    {
        var workbookStylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
        workbookStylesPart.Stylesheet = new Stylesheet();

        var stylesheet = workbookStylesPart.Stylesheet;

        // Initialize NumberingFormats
        stylesheet.NumberingFormats = new NumberingFormats();

        // Initialize Fonts (at least one is required)
        stylesheet.Fonts = new Fonts(new Font())
        {
            Count = 1
        };

        // Initialize Fills (at least one is required)
        stylesheet.Fills = new Fills(new Fill(new PatternFill { PatternType = PatternValues.None }))
        {
            Count = 1
        };

        // Initialize Borders (at least one is required)
        stylesheet.Borders = new Borders(new Border())
        {
            Count = 1
        };

        // Initialize CellFormats
        stylesheet.CellFormats = new CellFormats();

        // Add default format (required)
        stylesheet.CellFormats.AppendChild(new CellFormat());

        // Define the date format
        var numberingFormat = new NumberingFormat
        {
            NumberFormatId = UInt32Value.FromUInt32(164), // Custom format IDs must be >= 164
            FormatCode = StringValue.FromString(configurationService.DateFormat)
        };
        stylesheet.NumberingFormats.AppendChild(numberingFormat);
        // Create a new CellFormat with the date format
        var dateFormat = new CellFormat
        {
            NumberFormatId = numberingFormat.NumberFormatId,
            ApplyNumberFormat = BooleanValue.FromBoolean(true)
        };

        stylesheet.CellFormats.AppendChild(dateFormat);

        // Define the number format
        numberingFormat = new NumberingFormat
        {
            NumberFormatId = UInt32Value.FromUInt32(165), // Custom format IDs must be >= 164
            FormatCode = StringValue.FromString(configurationService.NumberFormat)
        };
        stylesheet.NumberingFormats.AppendChild(numberingFormat);
        // Create a new CellFormat with the date format
        var numberFormat = new CellFormat
        {
            NumberFormatId = numberingFormat.NumberFormatId,
            ApplyNumberFormat = BooleanValue.FromBoolean(true)
        };
        stylesheet.CellFormats.AppendChild(numberFormat);

        // Define the URL format
        var font = new Font(
            new Color { Theme = 10U },
            new Underline { Val = UnderlineValues.Single }
        );

        stylesheet.Fonts.AppendChild(font);

        var cellFormat = new CellFormat
        {
            FontId = stylesheet.Fonts.Count - 1,
            ApplyFont = true
        };

        stylesheet.CellFormats.AppendChild(cellFormat);

        stylesheet.CellFormats.Count = 4; // Default + Date format + Number format + URL format
    }

    private static bool IsCurrencyString(string value)
    {
        return value.StartsWith("EUR", StringComparison.OrdinalIgnoreCase) ||
               value.StartsWith("USD", StringComparison.OrdinalIgnoreCase);
    }
}