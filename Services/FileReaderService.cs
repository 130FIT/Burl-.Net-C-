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
                Console.WriteLine($"Error reading file: {ex.Message}");
                throw new Exception($"Error reading file: {ex.Message}");
            }
        }
        private async Task<T> ReadFileAsync<T>(string filePath, string destinationPath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found", filePath);
            }
            try
            {
                File.Copy(filePath, destinationPath, true);
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string json = await reader.ReadToEndAsync();

                    return JsonSerializer.Deserialize<T>(json) ?? throw new Exception("Deserialization failed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
                throw new Exception($"Error reading file: {ex.Message}");
            }
        }
        public Task<ApiRequest> ReadTestFileAsync(string filePath,string directoryPath)
        {
            Directory.CreateDirectory(Path.Combine(directoryPath, "source"));
            return ReadFileAsync<ApiRequest>(filePath, Path.Combine(directoryPath, "source", Path.GetFileName(filePath)));
        }

        public Task<RunnerFileRequest> ReadRunnerFileAsync(string filePath)
        {
            RunnerFileRequest runner = ReadFileAsync<RunnerFileRequest>(filePath).Result;
            runner.TestFiles.ForEach(testFile =>
            {
                string? directoryName = Path.GetDirectoryName(filePath);
                if (directoryName != null)
                {
                    testFile.File = Path.Combine(directoryName, testFile.File);
                }
            });
            return Task.FromResult(runner);
        }

    }
}
