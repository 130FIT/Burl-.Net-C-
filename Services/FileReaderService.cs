namespace Services
{
    using System;
    using System.IO;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Models;

    public class FileReaderService : IFileReaderService
    {
        private async Task<T> ReadFileAsync<T>(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found", filePath);
            }
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string json = await reader.ReadToEndAsync();
                    return JsonSerializer.Deserialize<T>(json) ?? throw new Exception("Deserialization failed");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading file: {ex.Message}");
            }
        }

        public Task<ApiRequest> ReadTestFileAsync(string filePath)
        {
            return ReadFileAsync<ApiRequest>(filePath);
        }

        public Task<RunnerFileRequest> ReadRunnerFileAsync(string filePath)
        {
            return ReadFileAsync<RunnerFileRequest>(filePath);
        }
    }
}
