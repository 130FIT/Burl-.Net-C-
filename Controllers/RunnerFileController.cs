using Models;
using Services;
using System.Threading.Tasks;

namespace Controllers
{
    public class RunnerFileController
    {
        private readonly FileReaderService _fileReaderService;
        private RunnerFileRequest? _runnerFileRequest;

        public RunnerFileController(FileReaderService fileReaderService)
        {
            _fileReaderService = fileReaderService;
        }

        public async Task<bool> ReadRunnerFileAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            }

            try
            {
                _runnerFileRequest = await _fileReaderService.ReadRunnerFileAsync(filePath);
                Console.WriteLine($"\n {_runnerFileRequest.Mode} \n");
                return _runnerFileRequest != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading runner file: {ex.Message}");
                throw new Exception("Error reading runner file", ex);
            }
        }

        public RunnerFileRequest GetRunnerFileRequest()
        {
            if (_runnerFileRequest == null)
            {
                throw new InvalidOperationException("Runner file has not been read yet.");
            }
            return _runnerFileRequest;
        }
    }
}
