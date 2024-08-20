using System.Reflection;
using System.Text.Json;
namespace Helpers
{
    public class JsonHelper
    {
        public static string ToJsonString(Dictionary<string, object> json)
        {
            if (json == null) return "null";
            return $"{string.Join(", ", json.Select(kvp => $"\"{kvp.Key}\" : \"{kvp.Value}\""))}";
        }

        public static Dictionary<string, object> ObjectToDictionary(object? obj)
        {
            var dictionary = new Dictionary<string, object>();
            if (obj != null)
            {
                foreach (PropertyInfo property in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var value = property.GetValue(obj);
                    if (value != null)
                    {
                        dictionary.Add(property.Name, value);
                    }
                }
            }
            return dictionary;
        }
        public static void UpdateValue(Dictionary<string, object> data, string key, object value)
        {
            var keys = key.Split('.');
            object current = data;

            for (int i = 0; i < keys.Length; i++)
            {
                var k = keys[i];

                if (k.Contains("[") && k.Contains("]"))
                {
                    var sliceKey = k.Substring(0, k.IndexOf("["));
                    var indexStr = k.Substring(k.IndexOf("[") + 1, k.IndexOf("]") - k.IndexOf("[") - 1);
                    if (!int.TryParse(indexStr, out int index))
                    {
                        throw new ArgumentException($"Invalid index in key: {k}");
                    }

                    var currentDict = current as Dictionary<string, object>;
                    if (currentDict == null || !currentDict.ContainsKey(sliceKey))
                    {
                        throw new ArgumentException($"Invalid key: {sliceKey}");
                    }

                    var currentSlice = currentDict[sliceKey] as List<object>;
                    if (currentSlice == null || currentSlice.Count <= index)
                    {
                        throw new ArgumentException($"Invalid slice or index out of range for key: {k}");
                    }

                    if (i == keys.Length - 1)
                    {
                        // Update the value in the slice
                        var mapElem = currentSlice[index] as Dictionary<string, object>;
                        if (mapElem != null)
                        {
                            mapElem[keys[i + 1]] = value;
                            return;
                        }
                        throw new ArgumentException($"Element at index {index} is not a map");
                    }
                    current = currentSlice[index];
                }
                else
                {
                    if (i == keys.Length - 1)
                    {
                        var m = current as Dictionary<string, object>;
                        if (m != null)
                        {
                            m[k] = value;
                            return;
                        }
                        throw new ArgumentException("Final element is not a map");
                    }
                    var dict = current as Dictionary<string, object>;
                    if (dict != null && dict.ContainsKey(k))
                    {
                        current = dict[k];
                    }
                    else
                    {
                        throw new ArgumentException("Unexpected type, expected Dictionary<string, object>");
                    }
                }
            }

            throw new ArgumentException("Invalid key structure");
        }
        public static object? GetValue(Dictionary<string, object> data, string key)
        {
            var keys = key.Split('.');
            object current = data;

            try
            {

                for (int i = 0; i < keys.Length; i++)
                {
                    if (current.GetType() == typeof(System.Text.Json.JsonElement))
                    {
                        current = JsonElementToDictionary((JsonElement)current);
                    }
                    var k = keys[i];

                    if (k.Contains("[") && k.Contains("]"))
                    {
                        var sliceKey = k.Substring(0, k.IndexOf("["));
                        var indexStr = k.Substring(k.IndexOf("[") + 1, k.IndexOf("]") - k.IndexOf("[") - 1);
                        if (!int.TryParse(indexStr, out int index))
                        {
                            throw new ArgumentException($"Invalid index in key: {k}");
                        }

                        var currentDict = current as Dictionary<string, object>;
                        if (currentDict == null || !currentDict.ContainsKey(sliceKey))
                        {
                            throw new ArgumentException($"Invalid key: {sliceKey}");
                        }

                        var currentSlice = currentDict[sliceKey] as List<object>;
                        if (currentSlice == null || currentSlice.Count <= index)
                        {
                            throw new ArgumentException($"Invalid slice or index out of range for key: {k}");
                        }

                        if (i == keys.Length - 1)
                        {
                            // Return the value from the slice
                            return currentSlice[index];
                        }
                        current = currentSlice[index];
                    }
                    else
                    {
                        if (i == keys.Length - 1)
                        {
                            var m = current as Dictionary<string, object>;
                            if (m != null && m.ContainsKey(k))
                            {
                                return m[k];
                            }
                            throw new ArgumentException("Final element is not a map or key not found");
                        }
                        var dict = current as Dictionary<string, object>;
                        if (dict != null && dict.ContainsKey(k))
                        {
                            current = dict[k];
                        }
                        else
                        {
                            throw new ArgumentException("Unexpected type, expected Dictionary<string, object>");
                        }
                    }
                }

                throw new ArgumentException("Invalid key structure");
            }
            catch (ArgumentException e)
            {
                ConsoleHelper.PrintError(e.Message);
                return null;
            }
        }

        public static T DictionaryToObject<T>(Dictionary<string, object> dict) where T : new()
        {
            T obj = new T();
            foreach (var kvp in dict)
            {
                PropertyInfo? prop = obj.GetType().GetProperty(kvp.Key);
                if (prop != null && prop.CanWrite)
                {
                    var value = kvp.Value;
                    if (value == null && Nullable.GetUnderlyingType(prop.PropertyType) == null)
                    {
                        continue;
                    }

                    prop.SetValue(obj, value);
                }
            }
            return obj;
        }

        public static Dictionary<string, object> MergeDictionary(Dictionary<string, object>? baseDict, Dictionary<string, object>? testCaseDict)
        {
            // รวม baseDict และ testCaseDict แล้ว return
            Dictionary<string, object> mergedDict = new Dictionary<string, object>();
            if (baseDict != null)
            {
                foreach (var key in baseDict.Keys)
                {
                    mergedDict[key] = baseDict[key];
                }
            }
            if (testCaseDict != null)
            {
                foreach (var key in testCaseDict.Keys)
                {
                    mergedDict[key] = testCaseDict[key];
                }
            }
            return mergedDict;
        }
        public static (string sliceKey, int index) ParseKey(string key)
        {
            var sliceKey = key.Substring(0, key.IndexOf("["));
            var indexStr = key.Substring(key.IndexOf("[") + 1, key.IndexOf("]") - key.IndexOf("[") - 1);
            if (!int.TryParse(indexStr, out int index))
            {
                throw new ArgumentException($"Invalid index in key: {key}");
            }

            return (sliceKey, index);
        }
        public static Dictionary<string, object> ParseJson(string json)
        {
            var result = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            return result ?? new Dictionary<string, object>();
        }

        public static object? ConvertToActualType(JsonElement element)
        {   
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return element.GetString();
                case JsonValueKind.Number:
                    // ลองแปลงเป็น int หรือ double
                    if (element.TryGetInt32(out int intValue))
                    {
                        return intValue;
                    }
                    if (element.TryGetDouble(out double doubleValue))
                    {
                        return doubleValue;
                    }
                    return null;
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return element.GetBoolean();
                case JsonValueKind.Object:
                    // หากต้องการแปลง Object เป็น Dictionary หรือ Object ที่กำหนดเอง
                    return JsonSerializer.Deserialize<Dictionary<string, object>>(element.GetRawText());
                case JsonValueKind.Array:
                    // หากต้องการแปลง Array เป็น List
                    return JsonSerializer.Deserialize<List<object>>(element.GetRawText());
                default:
                    return null;
            }
        }
        public static Dictionary<string, object> JsonElementToDictionary(JsonElement element)
        {
            var dict = new Dictionary<string, object>();

            foreach (JsonProperty property in element.EnumerateObject())
            {
                switch (property.Value.ValueKind)
                {
                    case JsonValueKind.Object:
                        dict[property.Name] = JsonElementToDictionary(property.Value);
                        break;
                    case JsonValueKind.Array:
                        dict[property.Name] = JsonElementToArray(property.Value);
                        break;
                    case JsonValueKind.String:
                        dict[property.Name] = property.Value.GetString() ?? property.Value.ToString();
                        break;
                    case JsonValueKind.Number:
                        dict[property.Name] = property.Value.GetDecimal(); // หรือใช้ GetInt32(), GetDouble() ตามที่เหมาะสม
                        break;
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        dict[property.Name] = property.Value.GetBoolean();
                        break;
                    case JsonValueKind.Null:
                        dict[property.Name] = "";
                        break;
                    default:
                        dict[property.Name] = property.Value.ToString();
                        break;
                }
            }

            return dict;
        }

        private static List<object> JsonElementToArray(JsonElement element)
        {
            var list = new List<object>();

            foreach (JsonElement item in element.EnumerateArray())
            {
                switch (item.ValueKind)
                {
                    case JsonValueKind.Object:
                        list.Add(JsonElementToDictionary(item));
                        break;
                    case JsonValueKind.Array:
                        list.Add(JsonElementToArray(item));
                        break;
                    case JsonValueKind.String:
                        list.Add(item.GetString()?? item.ToString());
                        break;
                    case JsonValueKind.Number:
                        list.Add(item.GetDecimal());
                        break;
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        list.Add(item.GetBoolean());
                        break;
                    case JsonValueKind.Null:
                        list.Add(string.Empty);
                        break;
                    default:
                        list.Add(item.ToString());
                        break;
                }
            }

            return list;
        }
    }
}