using FuzzySharp.Extractor;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility.Search;

public static class FuzzySearch
{
    public static ExtractedResult<string> ExtractOneWithParts(string queryStr, IEnumerable<string> values, int score)
    {
        var valuesList = values.IsNotNullOrEmpty().ToList();
        queryStr = queryStr.Trim();
        var formattedQueryStr = queryStr.ReformatToUpper();

        var directlyMatch =
            valuesList
            .Select(_ => new
            {
                Orginal = _,
                Formatted = _.ReformatToUpper()
            })
            .Where(_ => _.Formatted == queryStr ||
                      _.Formatted.Contains(formattedQueryStr) ||
                      formattedQueryStr.Contains(_.Formatted))
            .ToList();

        if (directlyMatch.Any())
        {
            var result = FuzzySharp.Process.ExtractOne(queryStr, directlyMatch.Select(_=>_.Orginal));

            int index = valuesList.FindIndex(_ => _ == result.Value);
            return new ExtractedResult<string>(result.Value, result.Score, index);
        }

        var res = FuzzySharp.Process.ExtractOne(queryStr, valuesList);

        if (res != null && res.Score > score)
        {
            return res;
        }

        var queryStrParts = queryStr.Split(new[] { " ", "_" }, StringSplitOptions.RemoveEmptyEntries);

        if (queryStrParts.Count() > 1)
        {
            var resultParts = new List<ExtractedResult<string>>();

            foreach (var queryStrPart in queryStrParts)
            {
                var result = FuzzySharp.Process.ExtractOne(queryStr, valuesList);

                if (res != null && res.Score > score)
                    resultParts.Add(result);
            }

            var part = resultParts.OrderByDescending(_ => _.Score).FirstOrDefault();

            if (part != null)
                return null;

            return part;
        }

        return null;
    }
}
