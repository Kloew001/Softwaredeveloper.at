using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using Serilog.Events;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Utility;

internal class SerilogUtility
{
}

public static class LoggingFilters
{
    public static bool IsArea(LogEvent evt, string area)
    {
        if (evt == null) return false;
        if (!evt.Properties.TryGetValue("Area", out var areaValue))
            return false;

        if (areaValue is ScalarValue sv && sv.Value != null)
        {
            return string.Equals(
                sv.Value.ToString(),
                area,
                StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    public static bool IsAreaAny(LogEvent evt, params string[] areas)
    {
        if (evt == null || areas == null || areas.Length == 0)
            return false;

        if (!evt.Properties.TryGetValue("Area", out var areaValue))
            return false;

        if (areaValue is ScalarValue sv && sv.Value != null)
        {
            var value = sv.Value.ToString();
            foreach (var a in areas)
            {
                if (string.Equals(value, a, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }

        return false;
    }

    public static bool IsWeb(LogEvent evt) => IsArea(evt, "Web");

    public static bool IsWorker(LogEvent evt) => IsArea(evt, "Worker");

    public static bool IsWebOrWorker(LogEvent evt) => IsAreaAny(evt, "Web", "Worker");

    public static Func<LogEvent, bool> AreaEquals(string area) =>
        evt => IsArea(evt, area);

    public static Func<LogEvent, bool> ExcludeAreas(params string[] areas) =>
        evt => IsAreaAny(evt, areas);
}
