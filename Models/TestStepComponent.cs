using System.Runtime.CompilerServices;
using Models;

public class TestStepComponent
{
    public string? Step { get; set; }
    public string? File { get; set; }
    public string? TestType { get; set; }
    public int CaseID { get; set; }
    public string? CaseName { get; set; }
    public string? CaseDescription { get; set; }
    public List<string>? CaseTags { get; set; }
    public string? RequestURL { get; set; }
    public string? RequestMethod { get; set; }
    public string? RequestType { get; set; }
    public Dictionary<string, List<string>>? RequestHeader { get; set; }
    public object? RequestBody { get; set; }
    public string? RequestSource { get; set; }
    public int ResponseStatusCode { get; set; }
    public object? ResponseHeader { get; set; }
    public string? ResponseType { get; set; }
    public object? ResponseBody { get; set; }
    public Dictionary<string, object>? CapturedValues { get; set; }
    public string? TestStatus { get; set; }
    public string? Error { get; set; }
    public AssertComponent? Assert { get; set; }
    public string? ResponseSource { get; set; }
}


