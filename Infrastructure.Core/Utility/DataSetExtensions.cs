using System.Data;
using System.Reflection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

[AttributeUsage(AttributeTargets.Property)]
public class DataSetColumnAttribute : Attribute
{
    public DataSetColumnAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; }
}

public static class DataSetExtensions
{
    public static DataSet ToDataSet<T>(this IEnumerable<T> items)
    {
        var dataSet = new DataSet();
        var dataTable = new DataTable();
        var schemaInitialized = false;

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

    private static string GetColumnName(PropertyInfo property)
    {
        var columnNameAttribute = property.GetCustomAttribute<DataSetColumnAttribute>();
        return columnNameAttribute?.Name ?? property.Name;
    }

    private static void InitializeDataTableSchema(this DataTable dataTable, object record)
    {
        var properties = record.GetType().GetProperties();
        foreach (var prop in properties)
        {
            var propertyType = prop.PropertyType;
            if (Nullable.GetUnderlyingType(propertyType) != null)
                propertyType = Nullable.GetUnderlyingType(propertyType);

            var columnName = GetColumnName(prop);

            if (propertyType.IsEnum)
                dataTable.Columns.Add(columnName, typeof(string));
            else
                dataTable.Columns.Add(columnName, propertyType);
        }
    }

    private static void AddRecordToDataTable(this DataTable dataTable, object record)
    {
        var row = dataTable.NewRow();
        var properties = record.GetType().GetProperties();
        foreach (var prop in properties)
        {
            var value = prop.GetValue(record, null) ?? DBNull.Value;

            var propertyType = prop.PropertyType;
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

            var columnName = GetColumnName(prop);
            row[columnName] = value;
        }
        dataTable.Rows.Add(row);
    }
}