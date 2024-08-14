namespace Interfaces
{
    using System.Threading.Tasks;

    public interface IHttpService
    {
        Task<string> GetAsync(string url);
        Task<string> PostAsync(string url, string content);
        Task<string> PutAsync(string url, string content);
        Task<string> DeleteAsync(string url);
    }

}