
using Controllers;
using Helpers;
using Models;
using Services;


int mode = 0; // 0 = unit test, 1 = integration test
List<TestFileRunnerComponent> files;
FileReaderService fileReaderService = new FileReaderService();
if (args.Length > 0)
{
    string firstArg = args[0].ToLower();

    switch (firstArg)
    {
        case "-help":
        case "--h":
            CliHelper.ShowHelp();
            return;
        case "-version":
        case "--v":
            CliHelper.ShowVersion();
            return;
        case "--r":
        case "-runner":
            Console.WriteLine("Test with runner file mode enabled.");
            string fullPath = Path.GetFullPath(args[1]);
            // get only the directory path
            string directoryPath = Path.GetDirectoryName(fullPath) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                Console.WriteLine("Invalid directory path.");
                return;
            }
            RunnerFileController runnerFileController = new RunnerFileController(fileReaderService);
            runnerFileController.ReadRunnerFile(args[1]);
            RunnerFileRequest runnerFileRequest = runnerFileController.GetRunnerFileRequest();
            Console.WriteLine($"Mode: {runnerFileRequest?.Mode?.ToUpper()}");
            mode = runnerFileRequest?.Mode?.ToUpper() == "i" ? 1 : 0;
            files = runnerFileRequest?.TestFiles ?? new List<TestFileRunnerComponent>();
            Console.WriteLine($"Files: {files}");
            break;
        case "--i":
        case "-integration":
            mode = 1;
            files = CliHelper.ParseTestFile(args.Skip(1).ToArray());
            break;
        case "--u":
        case "-unit":
            Console.WriteLine("\nUnit test mode enabled.\n");
            mode = 0;
            files = CliHelper.ParseTestFile(args.Skip(1).ToArray());
            break;
        default:
            Console.WriteLine("\nInvalid argument. Use -help or --h for help.\n");
            return;
    }
}
else
{
    Console.WriteLine("\nNo arguments provided. Use -help or --h for help.\n");
    return;
}
FileWriterService fileWriterService = new FileWriterService();
TestController testController = new TestController(fileReaderService, new HttpService(fileWriterService), fileWriterService);
switch (mode)
{
    case 0:
        testController.UnitTesting(files);
        break;
    case 1:
        testController.IntegrationTesting(files);
        break;
}
