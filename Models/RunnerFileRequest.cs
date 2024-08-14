using System.Text.Json.Serialization;

namespace Models
{
    public class RunnerFileRequest
    {
        [JsonPropertyName("mode")] public string? Mode { get; set; }
        [JsonPropertyName("tests")] public required List<TestFileRunnerComponent> TestFiles { get; set; }
    }
}