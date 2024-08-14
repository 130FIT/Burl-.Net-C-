using System.Text.Json.Serialization;
using Helpers;
namespace Models
{
    public class TestCaseComponent
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("description")] public string? Description { get; set; }
        [JsonPropertyName("tags")] public List<string>? Tags { get; set; }
        [JsonPropertyName("assert_status")] public int? Status { get; set; }
        [JsonPropertyName("request_xml")] public Dictionary<string, object>? RequestXml { get; set; }
        [JsonPropertyName("request")] public Dictionary<string, object>? RequestJson { get; set; }
        [JsonPropertyName("assert_response")] public Dictionary<string, object>? AssertResponse { get; set; }
    }
}