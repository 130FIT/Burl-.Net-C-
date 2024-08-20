using System.Net.Http.Json;
using System.Xml;
using Newtonsoft.Json;

namespace Helpers
{
    public class XmlHelper
    {
        public static Dictionary<string, object> XmlToDictionary(string? xmlString)
        {
            if (string.IsNullOrWhiteSpace(xmlString))
            {
                throw new ArgumentException("XML string cannot be null or empty.", nameof(xmlString));
            }

            var dictionary = new Dictionary<string, object>();
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlString);

            // Start conversion from the root element
            ConvertXmlNodeToDictionary(xmlDocument.DocumentElement, dictionary);

            return dictionary;
        }

        private static void ConvertXmlNodeToDictionary(XmlNode? node, Dictionary<string, object> dict)
        {
            if (node == null) return;

            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode == null) continue;

                if (childNode.HasChildNodes)
                {
                    XmlNode? firstChild = childNode.FirstChild;
                    if (firstChild != null && firstChild.NodeType == XmlNodeType.Element)
                    {
                        var childDict = new Dictionary<string, object>();
                        ConvertXmlNodeToDictionary(childNode, childDict);
                        dict[childNode.Name] = childDict;
                    }
                    else
                    {
                        // Add the node value to the dictionary
                        dict[childNode.Name] = childNode.InnerText ?? string.Empty;
                    }
                }
                else
                {
                    // Add the node value to the dictionary if it has no children
                    dict[childNode.Name] = childNode.InnerText ?? string.Empty;
                }
            }
        }
        public static string DictionaryToXml(Dictionary<string, object> dict)
        {
            if (dict == null)
            {
                throw new ArgumentNullException(nameof(dict));
            }

            var xmlDocument = new XmlDocument();
            var root = xmlDocument.CreateElement("Root"); // สามารถเปลี่ยนเป็นชื่อ Root ที่ต้องการ
            xmlDocument.AppendChild(root);

            ConvertDictionaryToXmlElement(dict, root, xmlDocument);

            return xmlDocument.OuterXml;
        }

        public static string XmlToJson(string xmlString)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlString);
            return JsonConvert.SerializeXmlNode(xmlDocument);
        }
        private static void ConvertDictionaryToXmlElement(Dictionary<string, object> dict, XmlElement parentElement, XmlDocument xmlDocument)
        {
            foreach (var kvp in dict)
            {
                var key = kvp.Key;
                var value = kvp.Value;

                XmlElement element = xmlDocument.CreateElement(key);

                if (value is Dictionary<string, object> childDict)
                {
                    // Recursive call for nested dictionary
                    ConvertDictionaryToXmlElement(childDict, element, xmlDocument);
                }
                else
                {
                    element.InnerText = value?.ToString() ?? string.Empty;
                }

                parentElement.AppendChild(element);
            }
        }

    }
}