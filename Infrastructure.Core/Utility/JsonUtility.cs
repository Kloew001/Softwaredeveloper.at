using System.Text.Json.Nodes;

using Newtonsoft.Json;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

public static class JsonUtility
{
    public static string ToJson<T>(this T obj, JsonSerializerSettings settings = null)
    {
        return Newtonsoft.Json.JsonConvert.SerializeObject(obj, settings);
    }

    public static T FromJson<T>(this string json, JsonSerializerSettings settings = null)
    {
        if (json == null)
            return default;

        return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json, settings);
    }

    public static class JsonComparer
    {
        public static bool AreEqual(string leftJson, string rightJson, JsonComparisonOptions? options = null)
        {
            return GetDifferences(leftJson, rightJson, options).Count == 0;
        }

        public static bool AreEqual(JsonNode? left, JsonNode? right, JsonComparisonOptions? options = null)
        {
            return GetDifferences(left, right, options).Count == 0;
        }

        public static IReadOnlyList<string> GetDifferences(string leftJson, string rightJson, JsonComparisonOptions? options = null)
        {
            var leftNode = JsonNode.Parse(leftJson);
            var rightNode = JsonNode.Parse(rightJson);

            return GetDifferences(leftNode, rightNode, options);
        }

        public static IReadOnlyList<string> GetDifferences(JsonNode? left, JsonNode? right, JsonComparisonOptions? options = null)
        {
            options ??= new JsonComparisonOptions();

            var differences = new List<string>();
            CompareInternal(left, right, options, "$", differences);

            return differences;
        }

        private static void CompareInternal(JsonNode? left, JsonNode? right, JsonComparisonOptions options, string path, List<string> differences)
        {
            if (left is null && right is null)
                return;

            if (left is null || right is null)
            {
                differences.Add($"{path}: one side is null and the other is not.");
                return;
            }

            if (left is JsonObject leftObject && right is JsonObject rightObject)
            {
                CompareObjects(leftObject, rightObject, options, path, differences);
                return;
            }

            if (left is JsonArray leftArray && right is JsonArray rightArray)
            {
                CompareArrays(leftArray, rightArray, options, path, differences);
                return;
            }

            if (left is JsonValue && right is JsonValue)
            {
                var leftText = left.ToJsonString();
                var rightText = right.ToJsonString();

                if (!string.Equals(leftText, rightText, StringComparison.Ordinal))
                    differences.Add($"{path}: value differs (left={leftText}, right={rightText}).");

                return;
            }

            differences.Add($"{path}: node type differs (left={left.GetType().Name}, right={right.GetType().Name}).");
        }

        private static void CompareObjects(JsonObject left, JsonObject right, JsonComparisonOptions options, string path, List<string> differences)
        {
            var propertyNames = left.Select(p => p.Key)
                .Concat(right.Select(p => p.Key))
                .Distinct(StringComparer.OrdinalIgnoreCase);

            foreach (var propertyName in propertyNames)
            {
                var hasLeft = TryGetPropertyValueIgnoreCase(left, propertyName, out var leftValue);
                var hasRight = TryGetPropertyValueIgnoreCase(right, propertyName, out var rightValue);

                if (ShouldIgnoreProperty(propertyName, leftValue, rightValue, options))
                    continue;

                var propertyPath = $"{path}.{propertyName}";

                if (!hasLeft || !hasRight)
                {
                    differences.Add($"{propertyPath}: property missing on {(hasLeft ? "right" : "left")} side.");
                    continue;
                }

                CompareInternal(leftValue, rightValue, options, propertyPath, differences);
            }
        }

        private static void CompareArrays(JsonArray left, JsonArray right, JsonComparisonOptions options, string path, List<string> differences)
        {
            if (left.Count != right.Count)
                differences.Add($"{path}: array length differs (left={left.Count}, right={right.Count}).");

            var count = Math.Min(left.Count, right.Count);

            for (var i = 0; i < count; i++)
            {
                CompareInternal(left[i], right[i], options, $"{path}[{i}]", differences);
            }
        }

        private static bool TryGetPropertyValueIgnoreCase(JsonObject jsonObject, string propertyName, out JsonNode? value)
        {
            if (jsonObject.TryGetPropertyValue(propertyName, out value))
                return true;

            var actualName = jsonObject.Select(p => p.Key)
                .FirstOrDefault(k => string.Equals(k, propertyName, StringComparison.OrdinalIgnoreCase));

            if (actualName is null)
            {
                value = null;
                return false;
            }

            return jsonObject.TryGetPropertyValue(actualName, out value);
        }

        private static bool ShouldIgnoreProperty(string propertyName, JsonNode? leftValue, JsonNode? rightValue, JsonComparisonOptions options)
        {
            if (options.IgnoredPropertyNames.Contains(propertyName))
                return true;

            if (!options.IgnoreGuidDifferencesForIdProperties)
                return false;

            if (!IsIdProperty(propertyName))
                return false;

            return IsGuidValue(leftValue) && IsGuidValue(rightValue);
        }

        private static bool IsIdProperty(string propertyName)
        {
            if (propertyName.Equals("id", StringComparison.OrdinalIgnoreCase))
                return true;

            if (propertyName.EndsWith("Id", StringComparison.Ordinal))
                return true;

            if (propertyName.EndsWith("ID", StringComparison.Ordinal))
                return true;

            if (propertyName.EndsWith("_id", StringComparison.OrdinalIgnoreCase))
                return true;

            if (propertyName.EndsWith("-id", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        private static bool IsGuidValue(JsonNode? node)
        {
            if (node is null)
                return false;

            if (node is not JsonValue value)
                return false;

            if (!value.TryGetValue<string>(out var stringValue))
                return false;

            return Guid.TryParse(stringValue, out _);
        }
    }
}

public sealed class JsonComparisonOptions
{
    public ISet<string> IgnoredPropertyNames { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public bool IgnoreGuidDifferencesForIdProperties { get; set; } = true;
}
