
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
            RunnerFileController runnerFileController = new RunnerFileController(fileReaderService);
            runnerFileController.ReadRunnerFileAsync(args[1]).Wait();
            RunnerFileRequest runnerFileRequest = runnerFileController.GetRunnerFileRequest();
            mode = runnerFileRequest?.Mode?.ToUpper() == "I" ? 1 : 0;
            files = runnerFileRequest?.TestFiles ?? new List<TestFileRunnerComponent>();
            break;
        case "--i":
        case "-integration":
            Console.WriteLine("Integration test mode is not yet implemented.");
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
    string filesString = string.Join(", ", files.Select(f => f.File + ">" + string.Join(", ", f.Ids)));
    Console.WriteLine($"files: {filesString}");
}
else
{
    Console.WriteLine("\nNo arguments provided. Use -help or --h for help.\n");
    return;
}

TestController testController = new TestController(fileReaderService);
switch (mode)
{
    case 0:
        break;
    case 1:
        break;
}
