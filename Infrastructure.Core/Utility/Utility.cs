namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

public interface IRange<T>
{
    T Start { get; }
    T End { get; }
}

public class Range<T> : IRange<T>
{
    public Range(T start, T end)
    {
        Start = start;
        End = end;
    }

    public T Start { get; set; }
    public T End { get; set; }

    public override string ToString()
    {
        return $"{Start} - {End}";
    }
}