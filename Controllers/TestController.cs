using Services;

namespace Controllers
{
    public class TestController
    {
        private FileReaderService _fileReaderService;

        public TestController(FileReaderService fileReaderService)
        {
            _fileReaderService = fileReaderService;
        }

        public async Task UnitTest(List<string> files)
        {
            if (files.Count == 0)
            {
                Console.WriteLine("No files provided for unit test.");
                return;
            }
            foreach (string file in files)
            {
                Console.WriteLine($"Running unit test for {file}");
            }
        }
    }
}