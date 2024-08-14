using System.Text.Json.Serialization;

namespace Models
{
    public class ApiRequest
    {
        [JsonPropertyName("url")] public required string Url { get; set; }
        [JsonPropertyName("method")] public string? Method { get; set; }
        [JsonPropertyName("type")] public string? Type { get; set; }
        [JsonPropertyName("base_request_xml")] public string? BaseRequestXml { get; set; }
        [JsonPropertyName("base_request")] public Dictionary<string, object>? BaseRequestJson { get; set; }
        [JsonPropertyName("captures")] public List<CapturesComponent>? Captures { get; set; }
        [JsonPropertyName("cases")] public List<TestCaseComponent>? Cases { get; set; }
    }
}