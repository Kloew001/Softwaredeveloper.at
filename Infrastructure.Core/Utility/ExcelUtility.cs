using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Data;
using System.Globalization;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility
{
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
                headerValues.Add(cell.GetValue<string>());
            });

            return headerValues;
        }

        public static DataSet GetDataSetFromExcel(byte[] excelContent)
        {
            var dataSet = new DataSet();

            using var memoryStream = new MemoryStream(excelContent);
            using var workBook = new XLWorkbook(memoryStream);

            foreach (IXLWorksheet workSheet in workBook.Worksheets)
            {
                var dt = new DataTable(workSheet.Name);

                workSheet
                .FirstRowUsed()
                .CellsUsed()
                .ToList()
                .ForEach(cell =>
                {
                    dt.Columns.Add(cell.GetValue<string>());
                });

                foreach (IXLRow row in workSheet.RowsUsed().Skip(1))
                {
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        dr[i] = row.Cell(i + 1).Value.ToString();
                    }
                    dt.Rows.Add(dr);
                }

                dataSet.Tables.Add(dt);
            }

            return dataSet;
        }
    }
}
