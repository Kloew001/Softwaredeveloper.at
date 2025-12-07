using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

public static class OpenXmlExcelUtility
{
    public static MemoryStream LoadFromTemplateAndSaveToMemory(string templatePath)
    {
        var memoryStream = new MemoryStream();

        using (var templateStream = new FileStream(templatePath, FileMode.Open, FileAccess.Read))
        {
            templateStream.CopyTo(memoryStream);
        }

        memoryStream.Position = 0;

        return memoryStream;
    }

    public static SpreadsheetDocument OpenSpreadsheet(this Stream stream, bool iseditable = true)
    {
        return SpreadsheetDocument.Open(stream, iseditable);
    }

    public static Worksheet GetWorkSheet(this SpreadsheetDocument spreadSheet, string sheetName)
    {
        var workbookPart = spreadSheet.WorkbookPart;
        var sheet = workbookPart.Workbook.Descendants<Sheet>()
            .FirstOrDefault(s => s.Name == sheetName);

        var workSheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
        var workSheet = workSheetPart.Worksheet;

        return workSheet;
    }

    public static Cell Cell(this Worksheet worksheet, string cellName)
    {
        return worksheet.Descendants<Cell>()
            .FirstOrDefault(c => string.Compare(c.CellReference.Value, cellName, true) == 0);
    }

    public static Cell Cell(this Worksheet worksheet, int row, string letter)
    {
        return worksheet.Cell($"{letter}{row}");
    }

    public static void SetValue(this Cell cell, string value)
    {
        cell.CellValue = new CellValue(value);
        cell.DataType = new DocumentFormat.OpenXml.EnumValue<CellValues>(CellValues.String);
    }
}