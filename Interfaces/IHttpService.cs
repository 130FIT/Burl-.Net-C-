namespace Interfaces
{
    using System.Threading.Tasks;

    public interface IHttpService
    {
        HttpResponseMessage Request(string url, Dictionary<string, object> headers, string? method, string? type, object body,string step);

    }

}