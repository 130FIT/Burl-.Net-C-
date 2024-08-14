using System.Text.Json.Serialization;

namespace Models
{
    public class CapturesComponent
    {
        [JsonPropertyName("capture_path")] public required string CapturePath { get; set; }
        [JsonPropertyName("pass_path")] public required string PassPath { get; set; }
    }
}