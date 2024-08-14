namespace Helpers
{
    public class JsonHelper
    {
        public static string ToJsonString(Dictionary<string, object> json)
        {
            if (json == null) return "null";
            return $"{string.Join(", ", json.Select(kvp => $"\"{kvp.Key}\" : \"{kvp.Value}\""))}";
        }
    }
}