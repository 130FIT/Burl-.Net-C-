namespace Services
{
    using System.Threading.Tasks;
    using Models;

    public interface IFileReaderService
    {
        Task<ApiRequest> ReadTestFileAsync(string filePath, string destinationPath);
        Task<RunnerFileRequest> ReadRunnerFileAsync(string filePath);

    }
}