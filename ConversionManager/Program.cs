using Core;
using MenuLib;
using MenuImplementation;

namespace ConversionManager;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
    
        if (!TryResolveWorkerExePath(out string workerExePath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[ERROR] Executable file MockConverter.exe not found.");
            Console.ResetColor();
            Console.WriteLine("\nPlease build the MockConverter project before running the main application.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
            return;
        }
        
        var manager = new JobManager(workerExePath);
        MenuRunner.Run(new AppMainMenu(manager));
    }

    private static bool TryResolveWorkerExePath(out string workerExePath)
    {
        string primary = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MockConverter.exe");
        if (File.Exists(primary))
        {
            workerExePath = primary;
            return true;
        }

    #if DEBUG
        string config = "Debug";
    #else
        string config = "Release";
    #endif

        string netVersion = $"net{Environment.Version.Major}.{Environment.Version.Minor}";

        string fallback = Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..",
            "MockConverter", "bin", config, netVersion, "MockConverter.exe"));

        if (File.Exists(fallback))
        {
            workerExePath = fallback;
            return true;
        }

        workerExePath = string.Empty;
        return false;
    }
}