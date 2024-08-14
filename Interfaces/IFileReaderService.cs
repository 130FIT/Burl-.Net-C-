namespace Services
{
    using System.Threading.Tasks;
    using Models;

    public interface IFileReaderService
    {
        Task<ApiRequest> ReadTestFileAsync(string filePath);
        Task<RunnerFileRequest> ReadRunnerFileAsync(string filePath);

    }
}