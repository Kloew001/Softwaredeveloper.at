namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility
{

    public interface ISupportMultilingualDisplayName : ISupportDisplayName
    {
    }

    public interface ISupportDisplayName
    {
        string DisplayName { get; set; }
    }

    public static class IEnumerableExtensions
    {
        public static List<List<T>> GetAllCombinations<T>(List<T> list)
        {
            List<List<T>> result = new List<List<T>>();

            result.Add(new List<T>());

            result.Last().Add(list[0]);
            if (list.Count == 1)
                return result;

            List<List<T>> tailCombos = GetAllCombinations(list.Skip(1).ToList());
            tailCombos.ForEach(combo =>
            {
                result.Add(new List<T>(combo));
                combo.Add(list[0]);
                result.Add(new List<T>(combo));
            });
            return result;
        }

        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
        {
            var batch = new List<T>(batchSize);
            foreach (var item in source)
            {
                batch.Add(item);
                if (batch.Count == batchSize)
                {
                    yield return batch;
                    batch = new List<T>(batchSize);
                }
            }
            if (batch.Count > 0)
                yield return batch;
        }

        public static IEnumerable<T> Convert<T>(this System.Collections.IEnumerable source)
        {
            foreach (var item in source)
            {
                yield return (T)item;
            }
        }

        public static IEnumerable<T> OrderByDisplayName<T>(this IEnumerable<T> query)
            where T : ISupportDisplayName
        {
            return query.OrderBy(_ => _.DisplayName);
        }

        public static IEnumerable<string> IsNotNullOrEmpty(this IEnumerable<string> source)
        {
            return source.Where(_ => _.IsNullOrEmpty() == false);
        }

        public static IEnumerable<T> IsNotNull<T>(this IEnumerable<T> source)
        {
            return source.Where(_ => _ != null);
        }

        public static IEnumerable<T> OrderByRandom<T>(this IEnumerable<T> source)
        {
            var randomizer = new Random();
            return source.OrderBy(_ => randomizer.Next());
        }

        public static T GetRandom<T>(this IEnumerable<T> source)
        {
            return source.OrderByRandom().FirstOrDefault();
        }
    }
}
