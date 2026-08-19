namespace ConversionManager;

class Program
{
    static void Main(string[] args)
    {
        string workerExePath = ResolveWorkerExePath();
        var manager = new JobManager(workerExePath);

        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== CONVERSION MANAGER ===");
            Console.ResetColor();
            Console.WriteLine("1. Add Job");
            Console.WriteLine("2. Monitor Progress (Live)");
            Console.WriteLine("3. Cancel One Job");
            Console.WriteLine("4. Cancel All Jobs");
            Console.WriteLine("5. Help Screen");
            Console.WriteLine("6. Exit");
            Console.Write("\nSelect option: ");

            switch (Console.ReadLine())
            {
                case "1": AddJob(manager); break;
                case "2": ShowMonitor(manager); break;
                case "3": CancelSingleJob(manager); break;
                case "4": CancelAllJobs(manager); break;
                case "5": ShowHelp(); break;
                case "6": return;
            }
        }
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
    
    private static void AddJob(JobManager manager) {}
    private static void ShowMonitor(JobManager manager) {}
    private static void CancelSingleJob(JobManager manager) {}
    private static void CancelAllJobs(JobManager manager) {}
    private static void ShowHelp() {}
}