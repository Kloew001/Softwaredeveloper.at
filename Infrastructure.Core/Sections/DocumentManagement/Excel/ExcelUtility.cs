using System.Data;

using ClosedXML.Excel;

public static class ExcelUtility
{
    public static IEnumerable<string> GetExcelHeaders(byte[] excelContent)
    {
        var headerValues = new List<string>();

        using var memoryStream = new MemoryStream(excelContent);
        using var workBook = new XLWorkbook(memoryStream);

        var workSheet = workBook.Worksheets.First();
        var headerRow = workSheet.FirstRowUsed();

        workSheet
        .FirstRowUsed()
        .CellsUsed()
        .ToList()
        .ForEach(cell =>
        {
            headerValues.Add(cell.GetString());
        });

        return headerValues;
    }

    public static DataSet GetDataSetFromExcel(byte[] excelContent)
    {
        var dataSet = new DataSet();

        using var memoryStream = new MemoryStream(excelContent);
        using var workBook = new XLWorkbook(memoryStream);

        foreach (var workSheet in workBook.Worksheets)
        {
            var dataTable = new DataTable(workSheet.Name);

            workSheet
            .FirstRowUsed()
            .CellsUsed()
            .ToList()
            .ForEach(cell =>
            {
                var cellHeader = cell.GetString();
                //cell.DataType
                dataTable.Columns.Add(new DataColumn(cellHeader));
            });

            foreach (var row in workSheet.RowsUsed().Skip(1))
            {
                var dataRow = dataTable.NewRow();

                //foreach (IXLCell cell in row.Cells(row.FirstCellUsed().Address.ColumnNumber, row.LastCellUsed().Address.ColumnNumber))

                for (var i = 0; i < dataTable.Columns.Count; i++)
                {
                    var cell = row.Cell(i + 1);

                    if (cell.DataType == XLDataType.Text)
                        dataRow[i] = cell.GetString();
                    else if (cell.DataType == XLDataType.Number)
                        dataRow[i] = cell.GetDouble();
                    else if (cell.DataType == XLDataType.Boolean)
                        dataRow[i] = cell.GetBoolean();
                    else if (cell.DataType == XLDataType.DateTime)
                        dataRow[i] = cell.GetDateTime();
                    else if (cell.DataType == XLDataType.TimeSpan)
                        dataRow[i] = cell.GetTimeSpan();
                    else
                        dataRow[i] = cell.Value.ToString();
                }
                dataTable.Rows.Add(dataRow);
            }

            dataSet.Tables.Add(dataTable);
        }

        return dataSet;
    }

    public static byte[] GetExcelFromDataSet(this DataSet dataSet)
    {
        using var memoryStream = new MemoryStream();
        using var workBook = new XLWorkbook();

        foreach (DataTable dataTable in dataSet.Tables)
        {
            var workSheet = workBook.Worksheets.Add(dataTable.TableName);

            workSheet.Cell(1, 1).InsertTable(dataTable);
        }

        workBook.SaveAs(memoryStream);

        return memoryStream.ToArray();
    }
}