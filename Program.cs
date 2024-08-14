using System;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // ตรวจสอบว่า flag ถูกส่งเข้ามาหรือไม่
        if (args.Length > 0)
        {
            string firstArg = args[0].ToLower();

            switch (firstArg)
            {
                case "-help":
                    ShowHelp();
                    return;
                case "-version":
                    ShowVersion();
                    return;
                case "-i":
                    Console.WriteLine("Integration test mode is not yet implemented.");
                    return;
                case "-u":
                    Console.WriteLine("Unit test mode is not yet implemented.");
                    return;
            }
        }

        // ถ้าไม่มี flag ใดๆ ถูกส่งเข้ามา, ตรวจสอบว่ามี URL ถูกส่งเข้ามาหรือไม่
        if (args.Length == 0 || args.Length > 0 && args[0].StartsWith("-"))
        {
            Console.WriteLine("Please provide a URL as a command-line argument.");
            return;
        }

        string url = args[0];

        using (HttpClient client = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("Error in HTTP request: " + e.Message);
            }
        }
    }

    static void ShowHelp()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  Burl [URL]");
        Console.WriteLine("Options:");
        Console.WriteLine("  -help       Display help information.");
        Console.WriteLine("  -version    Display version information.");
    }

    static void ShowVersion()
    {
        Console.WriteLine("Burl version 1.0.0");
    }
}
