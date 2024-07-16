using Microsoft.EntityFrameworkCore;

using System.Data;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility
{
    public static class DataSetExtensions
    {
        public static async Task<DataSet> ToDataSetAsync<T>(this IQueryable<T> query)
        {
            var dataSet = new DataSet();
            var dataTable = new DataTable();
            bool schemaInitialized = false;

            var items = await query.ToListAsync();

            foreach (var item in items)
            {
                if (!schemaInitialized)
                {
                    dataTable.InitializeDataTableSchema(item);
                    schemaInitialized = true;
                }

                dataTable.AddRecordToDataTable(item);
            }

            dataSet.Tables.Add(dataTable);

            return dataSet;
        }

        public static void InitializeDataTableSchema(this DataTable dataTable, object record)
        {
            var properties = record.GetType().GetProperties();
            foreach (var prop in properties)
            {
                Type propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                dataTable.Columns.Add(prop.Name, propType);
            }
        }

        public static void AddRecordToDataTable(this DataTable dataTable, object record)
        {
            var row = dataTable.NewRow();
            var properties = record.GetType().GetProperties();
            foreach (var prop in properties)
            {
                row[prop.Name] = prop.GetValue(record) ?? DBNull.Value;
            }
            dataTable.Rows.Add(row);
        }
    }
}
