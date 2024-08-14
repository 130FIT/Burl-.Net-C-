using System.Text.Json.Serialization;

namespace Models
{
    public class TestFileRunnerComponent
    {
        [JsonPropertyName("display_name")] public string? DisplayName { get; set; }
        [JsonPropertyName("file")] public required string File { get; set; }
        [JsonPropertyName("id")] public List<object>? Ids { get; set; }
    }
}