using Interfaces;

namespace Services
{
    public class HttpService : HttpBaseService, IHttpService
    {
        public HttpService(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<string> GetAsync(string url)
        {
            return await SendRequestAsync(() => HttpClient.GetAsync(url));
        }

        public async Task<string> PostAsync(string url, string content)
        {
            HttpContent httpContent = CreateHttpContent(content);
            return await SendRequestAsync(() => HttpClient.PostAsync(url, httpContent));
        }

        public async Task<string> PutAsync(string url, string content)
        {
            HttpContent httpContent = CreateHttpContent(content);
            return await SendRequestAsync(() => HttpClient.PutAsync(url, httpContent));
        }

        public async Task<string> DeleteAsync(string url)
        {
            return await SendRequestAsync(() => HttpClient.DeleteAsync(url));
        }
    }
}
