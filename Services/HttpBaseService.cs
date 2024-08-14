using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public abstract class HttpBaseService
    {
        protected readonly HttpClient HttpClient;

        protected HttpBaseService(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        protected async Task<string> SendRequestAsync(Func<Task<HttpResponseMessage>> sendRequest)
        {
            try
            {
                HttpResponseMessage response = await sendRequest();
                return await ReadResponseContentAsync(response);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error during HTTP request: {ex.Message}");
            }
        }

        protected HttpContent CreateHttpContent(string content, string mediaType = "application/json")
        {
            return new StringContent(content, Encoding.UTF8, mediaType);
        }
        private async Task<string> ReadResponseContentAsync(HttpResponseMessage response)
        {   
            using (Stream responseStream = await response.Content.ReadAsStreamAsync())
            {
                using (StreamReader reader = new StreamReader(responseStream, Encoding.UTF8))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }
    }
}
