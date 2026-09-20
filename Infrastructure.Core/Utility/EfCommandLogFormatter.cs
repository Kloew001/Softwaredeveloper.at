using System.Data.Common;
using System.Globalization;
using System.Text;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

internal static class EfCommandLogFormatter
{
    private const int MaxParameters = 128;

    public static string FormatParameters(DbParameterCollection parameters, bool enableSensitiveDataLogging)
    {
        if (parameters.Count == 0)
            return string.Empty;

        var builder = new StringBuilder();
        var count = enableSensitiveDataLogging
            ? parameters.Count
            : Math.Min(parameters.Count, MaxParameters);

        for (var index = 0; index < count; index++)
        {
            if (index != 0)
                builder.Append(", ");

            if (!enableSensitiveDataLogging)
            {
                builder.Append(CultureInfo.InvariantCulture, $"@p{index}='?'");
                continue;
            }

            var parameter = parameters[index];

            builder.Append(Escape(parameter.ParameterName));
            builder.Append('=');
            builder.Append(FormatValue(parameter.Value));
            builder.Append(CultureInfo.InvariantCulture,
                $" (DbType = {parameter.DbType}) (Direction = {parameter.Direction}) (Nullable = {parameter.IsNullable}) (Size = {parameter.Size}) (Precision = {parameter.Precision}) (Scale = {parameter.Scale})");
        }

        if (parameters.Count > count)
            builder.Append(", [additional parameters omitted]");

        return builder.ToString();
    }

    private static string FormatValue(object value)
    {
        return value switch
        {
            null or DBNull => "NULL",
            byte[] bytes => "0x" + Convert.ToHexString(bytes),
            DateTime dateTime => Quote(dateTime.ToString("O", CultureInfo.InvariantCulture)),
            DateTimeOffset dateTimeOffset => Quote(dateTimeOffset.ToString("O", CultureInfo.InvariantCulture)),
            DateOnly date => Quote(date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
            TimeOnly time => Quote(time.ToString("O", CultureInfo.InvariantCulture)),
            TimeSpan duration => Quote(duration.ToString("c", CultureInfo.InvariantCulture)),
            Array array => FormatArray(array),
            IFormattable formattable => Quote(formattable.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty),
            _ => Quote(value.ToString() ?? string.Empty)
        };
    }

    private static string FormatArray(Array array)
    {
        var values = new List<string>(array.Length);

        foreach (var value in array)
            values.Add(FormatValue(value));

        return "[" + string.Join(", ", values) + "]";
    }

    private static string Quote(string value)
        => "'" + Escape(value) + "'";

    private static string Escape(string value)
        => value
            .Replace("\\", "\\\\")
            .Replace("'", "''")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n")
            .Replace("\t", "\\t");
}
