using Core;
using MenuLib;
using MenuImplementation;

namespace ConversionManager;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        
        string workerExePath = ResolveWorkerExePath();
        var manager = new JobManager(workerExePath);

        MenuRunner.Run(new AppMainMenu(manager));
    }

    private static string ResolveWorkerExePath() // build MockConverter first so it can find MockConverter.exe
    {
        string primary = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MockConverter.exe");
        if (File.Exists(primary)) return primary;
        
        string fallback = Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..",
            "MockConverter", "bin", "Debug", "net10.0", "MockConverter.exe")); // ATTENTION projects .net version is net10.0, if your version is different (.net 8) it will fail

        return File.Exists(fallback) ? fallback : primary;
    }
}