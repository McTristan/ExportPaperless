using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using ExportPaperless.Domain.Entities;
using ExportPaperless.Domain.Services;

namespace ExportPaperless.Services;

public class ExcelExportService : IExcelExportService
{
    public MemoryStream GenerateExcel(List<PaperlessDocument> documents, List<string> customFields)
    {
        var stream = new MemoryStream();
        using (var spreadsheet = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook, true))
        {
            var workbookPart = spreadsheet.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            var sheets = spreadsheet.WorkbookPart.Workbook.AppendChild(new Sheets());
            var sheet = new Sheet
            {
                Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "Documents"
            };
            sheets.Append(sheet);

            var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

            // Header row
            var header = new Row();
            header.Append(
                new Cell { CellValue = new CellValue("Title"), DataType = CellValues.String },
                new Cell { CellValue = new CellValue("Date"), DataType = CellValues.String },
                new Cell { CellValue = new CellValue("Tags"), DataType = CellValues.String },
                new Cell { CellValue = new CellValue("Correspondent"), DataType = CellValues.String },
                new Cell { CellValue = new CellValue("Notes"), DataType = CellValues.String },
                new Cell { CellValue = new CellValue("Document Type"), DataType = CellValues.String }
            );
            foreach (var fieldName in customFields)
            {
                header.Append(new Cell { CellValue = new CellValue(fieldName), DataType = CellValues.String });
            }

            sheetData.Append(header);

            foreach (var doc in documents)
            {
                var row = new Row();
                row.Append(
                    new Cell { CellValue = new CellValue(doc.Title), DataType = CellValues.String },
                    new Cell
                    {
                        CellValue = new CellValue(doc.Created.ToString("yyyy-MM-dd")), DataType = CellValues.String
                    },
                    new Cell { CellValue = new CellValue(string.Join(", ", doc.Tags ?? new List<string>())), DataType = CellValues.String },
                    new Cell { CellValue = new CellValue(doc.Correspondent ?? ""), DataType = CellValues.String },
                    new Cell { CellValue = new CellValue(string.Join(", ", doc.Notes ?? new List<string>())), DataType = CellValues.String },
                    new Cell { CellValue = new CellValue(doc.DocumentType ?? ""), DataType = CellValues.String }
                );
                foreach (var fieldName in customFields)
                {
                    doc.CustomFields?.TryGetValue(fieldName, out var value);
                    
                    //row.Append(new Cell
                    //    { CellValue = new CellValue(value ?? string.Empty), DataType = CellValues.String });
                    row.Append(new Cell());
                }

                sheetData.Append(row);
            }

            workbookPart.Workbook.Save();
        }

        stream.Position = 0;
        return stream;
    }
}