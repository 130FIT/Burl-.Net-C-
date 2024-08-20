namespace Models
{
    public class AssertComponent
    {   
        public List<SubAssertComponent>? ResponseBody { get; set; }
        public List<SubAssertComponent>? ResponseHeader { get; set; }
        public List<SubAssertComponent>? ResponseStatusCode { get; set; }
    }
}