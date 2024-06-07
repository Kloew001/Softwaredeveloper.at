using Newtonsoft.Json;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility
{
    public static class JsonUtility
    {
        public static string ToJson<T>(this T obj, JsonSerializerSettings settings = null)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj, settings);
        }

        public static T FromJson<T>(this string json, JsonSerializerSettings settings = null)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json, settings);
        }
    }
}
