using System.Security;
using Models;

namespace Helpers
{
    public static class CliHelper
    {
        public static void ShowHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  Burl [URL]");
            Console.WriteLine("Options:");
            Console.WriteLine("  -help       Display help information.");
            Console.WriteLine("  -version    Display version information.");
            Console.WriteLine("  -i          Integration test mode.");
            Console.WriteLine("  -u          Unit test mode.");
        }

        public static void ShowVersion()
        {
            Console.WriteLine("Burl version 1.0.0");
        }

        public static List<TestFileRunnerComponent> ParseTestFile(string[] files)
        {
            List<TestFileRunnerComponent> testFiles = new List<TestFileRunnerComponent>();
            foreach (string file in files)
            {
                string[] fileParts = file.Split(">");
                if (fileParts.Length == 1)
                {
                    testFiles.Add(new TestFileRunnerComponent { File = fileParts[0], Ids = new List<object> { "*" } });
                }
                else
                {
                    testFiles.Add(new TestFileRunnerComponent { File = fileParts[0], Ids = new List<object> { fileParts[1] } });
                }
            }
            return testFiles;
        }
    }
}