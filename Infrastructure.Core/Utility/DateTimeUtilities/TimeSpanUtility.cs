namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility.DateTimeUtilities;

public static class TimeSpanUtility
{
    public static TimeSpan GetJitteredInterval(this TimeSpan baseInterval, double jitterRatio = 0.2)
    {
        var jitterMs = baseInterval.TotalMilliseconds * jitterRatio;
        var offsetMs = (Random.Shared.NextDouble() * 2 - 1) * jitterMs;

        return TimeSpan.FromMilliseconds(Math.Max(0, baseInterval.TotalMilliseconds + offsetMs));
    }
}
