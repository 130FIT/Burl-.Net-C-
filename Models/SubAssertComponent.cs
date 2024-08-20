namespace Models
{
    public class SubAssertComponent
    {
        public string? Key { get; set; }
        public object? Expected { get; set; }
        public object? Actual { get; set; }
        public string? Operator { get; set; }
        public bool IsAssert { get; set; }
    }
}