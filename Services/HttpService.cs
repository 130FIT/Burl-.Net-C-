using System.Text;
using Interfaces;
using System.Text.Json;
namespace Services
{
    public class HttpService : IHttpService
    {
        private readonly IFileWriterService _fileWriterService;

        public HttpService(IFileWriterService fileWriterService)
        {
            _fileWriterService = fileWriterService;
        }
        public HttpResponseMessage Request(string url, Dictionary<string, object> headers, string? method, string? type, object body, string step)
        {
            Console.WriteLine($"Requesting {url} with method {method} and body \n{body}");
            using (var httpClient = new HttpClient())
            {
                var requestMessage = new HttpRequestMessage
                {
                    RequestUri = new Uri(url),
                    Method = GetHttpMethod(method),
                    Content = body != null ? new StringContent(body.ToString() ?? "", Encoding.UTF8, GetContentType(type)) : new StringContent(""),
                };
                try
                {
                    foreach (var (k, v) in headers)
                    {
                        // แปลง JsonElement เป็น string
                        string headerValue;
                        if (v is JsonElement jsonElement)
                        {
                            headerValue = jsonElement.GetString() ?? $"{jsonElement}"; // ใช้ GetString() สำหรับ JsonElement ที่เป็น string
                        }
                        else
                        {
                            headerValue = v.ToString() ?? $"{v}"; // ใช้ ToString() สำหรับค่าที่ไม่ใช่ JsonElement
                        }

                        // ข้าม Content-Type headers ที่นี่
                        if (string.Equals(k, "Content-Type", StringComparison.OrdinalIgnoreCase))
                        {
                            // ข้าม Content-Type headers
                            continue;
                        }
                        if (requestMessage.Headers.Contains(k))
                        {
                            requestMessage.Headers.Remove(k);
                        }
                        requestMessage.Headers.Add(k, headerValue);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e}");
                    throw new Exception("Error while adding headers");
                }
                _fileWriterService.WriteJson($"request\\request{step}.json", SerializeHttpRequestMessage(requestMessage));

                return httpClient.SendAsync(requestMessage).Result;
            }
        }
        private static object SerializeHttpRequestMessage(HttpRequestMessage request)
        {
            // สร้างแหล่งข้อมูลสำหรับ HttpRequestMessage
            var requestData = new
            {
                Method = request.Method.Method,
                RequestUri = request.RequestUri,
                Headers = request.Headers,
                Content = request.Content != null ? ReadContentAsString(request.Content).Result : null
            };

            // แปลงเป็น JSON
            return requestData;
        }

        private static async Task<string> ReadContentAsString(HttpContent content)
        {
            return await content.ReadAsStringAsync();
        }
        private string GetContentType(string? type)
        {
            switch (type)
            {
                case "json":
                    return "application/json";
                case "xml":
                    return "application/xml";
                case "form":
                    return "application/x-www-form-urlencoded";
                default:
                    return "";
            }
        }
        private HttpMethod GetHttpMethod(string? method)
        {
            switch (method?.ToUpper() ?? "")
            {
                case "POST":
                    return HttpMethod.Post;
                case "PUT":
                    return HttpMethod.Put;
                case "DELETE":
                    return HttpMethod.Delete;
                default:
                    return HttpMethod.Get;
            }
        }
    }

}
