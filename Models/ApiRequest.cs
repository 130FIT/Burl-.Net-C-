using System.Text.Json.Serialization;
using Helpers;

namespace Models
{
    public class ApiRequest
    {
        [JsonPropertyName("url")] public required string Url { get; set; }
        [JsonPropertyName("method")] public string? Method { get; set; }
        [JsonPropertyName("type")] public string? Type { get; set; }
        [JsonPropertyName("base_request_xml")] public string? BaseRequestXml { get; set; }
        [JsonPropertyName("headers")] public Dictionary<string, object>? BaseHeader { get; set; }
        [JsonPropertyName("base_request")] public Dictionary<string, object>? BaseRequest { get; set; }
        [JsonPropertyName("captures")] public List<CapturesComponent>? Captures { get; set; }
        [JsonPropertyName("cases")] public List<TestCaseComponent>? Cases { get; set; }
        public Dictionary<string, object> GetRequestBodyJson(TestCaseComponent testCase)
        {
            return JsonHelper.MergeDictionary(BaseRequest, testCase.RequestJson);
        }
        public string GetRequestBodyXml(TestCaseComponent testCase)
        {
            // รวม BaseRequestXml และ RequestXml ของ testCase แล้ว return
            if (string.IsNullOrEmpty(BaseRequestXml) && string.IsNullOrEmpty(testCase.RequestXml)) return "";
            else if (string.IsNullOrEmpty(BaseRequestXml)) return testCase.RequestXml ?? "";
            else if (string.IsNullOrEmpty(testCase.RequestXml)) return BaseRequestXml ?? "";
            else
            {
                Dictionary<string, object> requestBase = XmlHelper.XmlToDictionary(BaseRequestXml);
                Console.WriteLine($"BaseRequestXml: {BaseRequestXml}");
                Dictionary<string, object> requestTestCase = XmlHelper.XmlToDictionary(testCase.RequestXml);
                Console.WriteLine(JsonHelper.MergeDictionary(requestBase, requestTestCase));
                return XmlHelper.DictionaryToXml(JsonHelper.MergeDictionary(requestBase, requestTestCase));
            }
        }
        public Dictionary<string, object> GetRequestHeaders(TestCaseComponent testCase)
        {
            return JsonHelper.MergeDictionary(BaseHeader, JsonHelper.ObjectToDictionary(testCase.Headers));
        }
        public void SetType()
        {
            if (string.IsNullOrEmpty(Type))
            {
                if (BaseRequestXml != null || (Cases != null && !string.IsNullOrEmpty(Cases[0].RequestXml))) Type = "xml";
                else Type = "json";
            }
        }
    }
}