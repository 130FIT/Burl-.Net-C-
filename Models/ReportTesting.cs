using System.Text.Json.Serialization;


namespace Models
{
    public class ReportTesting
    {
        public string? Title { get; set; }
        public string? Status { get; set; }
        public string? Error { get; set; }
        public string? Duration { get; set; }

        [JsonPropertyName("DateTime")]
        public DateTime? _DateTime { get; set; }
        public List<TestStepComponent>? TestSteps { get; set; }
    }
}