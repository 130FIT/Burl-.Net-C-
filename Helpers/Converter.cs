using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Models;

namespace Helpers
{
    public static class Converter
    {
        public static Dictionary<string, List<string>> ConvertingDictionaryObjectToListString(Dictionary<string, object> originalDict)
        {
            Dictionary<string, List<string>> convertedDict = new Dictionary<string, List<string>>();
            foreach (var kvp in originalDict)
            {
                List<string> valuesList = new List<string>();

                if (kvp.Value is string stringValue)
                {
                    valuesList.Add(stringValue);
                }
                else if (kvp.Value is IEnumerable<object> objectList)
                {
                    foreach (var item in objectList)
                    {
                        if (item is string s)
                        {
                            valuesList.Add(s);
                        }
                        else
                        {
                            valuesList.Add(item.ToString() ?? string.Empty);
                        }
                    }
                }
                else
                {
                    valuesList.Add(kvp.Value.ToString() ?? string.Empty);
                }

                convertedDict[kvp.Key] = valuesList;
            }
            return convertedDict;
        }
        public static void UpdateValue(object model, string key, object value)
        {
            var keys = key.Split('.');

            object current = model;
            // Define the Unicode values for '[' and ']'
            int unicodeOpenBracket = 0x005B; // Unicode for '['
            int unicodeCloseBracket = 0x005D; // Unicode for ']'

            // Convert to char for comparison
            char openBracket = (char)unicodeOpenBracket;
            char closeBracket = (char)unicodeCloseBracket;
            for (int i = 0; i < keys.Length; i++)
            {
                string k = keys[i];
                try
                {
                    if (k.Contains("[") && k.Contains("]"))
                    {
                        // Handle list index notation
                        string listKey = k.Substring(0, k.IndexOf(openBracket));
                        string indexStr = k.Substring(k.IndexOf(openBracket) + 1, k.IndexOf(closeBracket) - k.IndexOf(openBracket) - 1);
                        if (!int.TryParse(indexStr, out int index))
                        {
                            throw new ArgumentException($"Invalid index '{indexStr}' in key: {k}");
                        }

                        if (current is IDictionary<string, object> dictionary)
                        {
                            if (dictionary.TryGetValue(listKey, out var listObj) && listObj is IList list)
                            {
                                if (index >= 0 && index < list.Count)
                                {
                                    if (i == keys.Length - 1)
                                    {
                                        list[index] = Convert.ChangeType(value, list[index]!.GetType());
                                    }
                                    else
                                    {
                                        current = list[index]!;
                                    }
                                }
                                else
                                {
                                    throw new ArgumentException($"Index '{index}' is out of range for key: {listKey}");
                                }
                            }
                            else
                            {
                                throw new ArgumentException($"Key '{listKey}' not found in dictionary");
                            }
                        }
                        else
                        {
                            throw new ArgumentException($"Current object is not a dictionary for key: {k}");
                        }
                    }
                    else
                    {
                        if (i == keys.Length - 1)
                        {
                            // Update the value in the model or dictionary
                            if (current is IDictionary<string, object> dict)
                            {
                                dict[k] = value;
                            }
                            else
                            {
                                SetPropertyValue(current, k, value);
                            }
                            return;
                        }
                        else
                        {
                            current = GetPropertyValue(current, k);
                            if (current == null)
                            {
                                throw new ArgumentException($"Key '{k}' not found in model");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Error processing key '{k}' at step {i}: {ex.Message}", ex);
                }
            }
        }

        private static object GetPropertyValue(object obj, string propertyName)
        {
            if (obj is IDictionary<string, object> dictionary)
            {
                return dictionary.TryGetValue(propertyName, out var value) ? value : null;
            }
            else
            {
                var property = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(p => GetJsonPropertyName(p) == propertyName);
                return property?.GetValue(obj);
            }
        }

        private static void SetPropertyValue(object obj, string propertyName, object value)
        {
            if (obj is IDictionary<string, object> dictionary)
            {
                dictionary[propertyName] = Convert.ChangeType(value, typeof(object));
            }
            else
            {
                var property = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(p => GetJsonPropertyName(p) == propertyName);

                if (property != null)
                {
                    if (property.CanWrite)
                    {
                        property.SetValue(obj, Convert.ChangeType(value, property.PropertyType));
                    }
                    else
                    {
                        throw new ArgumentException($"Property {propertyName} is read-only");
                    }
                }
                else
                {
                    throw new ArgumentException($"Property {propertyName} not found");
                }
            }
        }

        private static string GetJsonPropertyName(PropertyInfo property)
        {
            var attribute = property.GetCustomAttribute<JsonPropertyNameAttribute>();
            return attribute != null ? attribute.Name : property.Name;
        }

        public static object GetValueFromJson(string json, string key)
        {
            var jsonDocument = JsonDocument.Parse(json);
            var keys = key.Split('.');
            JsonElement element = jsonDocument.RootElement;

            foreach (var k in keys)
            {
                if (k.Contains("[") && k.Contains("]"))
                {
                    var keyName = k.Substring(0, k.IndexOf('['));
                    var indexStr = k.Substring(k.IndexOf('[') + 1, k.IndexOf(']') - k.IndexOf('[') - 1);

                    if (!int.TryParse(indexStr, out int index))
                    {
                        return null;
                    }

                    if (element.TryGetProperty(keyName, out JsonElement arrayElement) && arrayElement.ValueKind == JsonValueKind.Array)
                    {
                        if (index >= 0 && index < arrayElement.GetArrayLength())
                        {
                            element = arrayElement[index];
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    if (element.TryGetProperty(k, out JsonElement propertyElement))
                    {
                        element = propertyElement;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Object => element.GetRawText(),
                JsonValueKind.Array => element.GetRawText(),
                JsonValueKind.Undefined => throw new NotImplementedException(),
                JsonValueKind.Null => throw new NotImplementedException(),
                _ => null,
            };
        }

        public static (int mode, string key) ExtractModeAndKey(string value)
        {

            // กำหนดค่าเริ่มต้น
            int mode = 2; // ค่าเริ่มต้นคือ mode = 3
            string key = value;

            // ตรวจสอบว่ามีการใช้วงเล็บหรือไม่
            int startIndex = value.IndexOf('(');
            int endIndex = value.IndexOf(')');

            if (startIndex >= 0 && endIndex > startIndex)
            {
                // Extract mode from parentheses
                string modeString = value.Substring(startIndex + 1, endIndex - startIndex - 1);

                // กำหนดค่า mode ตามที่ระบุในวงเล็บ
                switch (modeString.ToLower())
                {
                    case "header":
                        mode = 1;
                        break;
                    case "body":
                        mode = 2;
                        break;
                    default:
                        mode = 2; // ใช้ค่าเริ่มต้นถ้าไม่ตรงกับค่าใด ๆ
                        break;
                }

                // Extract key after parentheses
                key = value.Substring(endIndex + 1).Trim();
            }
            else
            {
                // ไม่มีการใช้วงเล็บให้ใช้ key ตรง ๆ
                key = value.Trim();
            }

            return (mode, key);

        }

    }

}