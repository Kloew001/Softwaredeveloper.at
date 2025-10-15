using System.Data;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

public static class DataSetExtensions
{
    public static DataSet ToDataSet<T>(this IEnumerable<T> items)
    {
        var dataSet = new DataSet();
        var dataTable = new DataTable();
        bool schemaInitialized = false;

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
            Type propertyType = prop.PropertyType;
            if (Nullable.GetUnderlyingType(propertyType) != null)
                propertyType = Nullable.GetUnderlyingType(propertyType);

            if (propertyType.IsEnum)
                dataTable.Columns.Add(prop.Name, typeof(string));
            else
                dataTable.Columns.Add(prop.Name, propertyType);
        }
    }

    public static void AddRecordToDataTable(this DataTable dataTable, object record)
    {
        var row = dataTable.NewRow();
        var properties = record.GetType().GetProperties();
        foreach (var prop in properties)
        {
            object value = prop.GetValue(record, null) ?? DBNull.Value;

            Type propertyType = prop.PropertyType;
            if (Nullable.GetUnderlyingType(propertyType) != null)
                propertyType = Nullable.GetUnderlyingType(propertyType);

            if (propertyType.IsEnum && value != DBNull.Value)
            {
                value = value.ToString();
            }

            if (value is string strValue && strValue.Length > 1000)
            {
                value = strValue.Substring(0, 1000);
            }

            row[prop.Name] = value;
        }
        dataTable.Rows.Add(row);
    }
}
