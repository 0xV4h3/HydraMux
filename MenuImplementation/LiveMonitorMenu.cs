namespace MenuImplementation;

public class LiveMonitorMenu : Menu
{
    private readonly JobManager _manager;

    public LiveMonitorMenu(JobManager manager) : base("")
    {
        _manager = manager;

        ConfigureOptionSize(0);
    }

    public override NavigationResult InteractiveSelect()
    {
        Console.Clear();
        Console.CursorVisible = false;

        while (true)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.Backspace || key == ConsoleKey.LeftArrow || key == ConsoleKey.A)
                {
                    return NavigationResult.Back();
                }
            }

            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("=== LIVE MONITOR ===\n");
            Console.ResetColor();

            var snapshots = _manager.GetSnapshot();

            int runningCount = 0;
            int queuedCount = 0;

            foreach (var snapshot in snapshots)
            {
                if (snapshot.Status == JobStatus.Running) runningCount++;
                if (snapshot.Status == JobStatus.Queued) queuedCount++;

                Console.Write($"{snapshot.Id,-5}{Truncate(snapshot.Input, 13),-15}{Truncate(snapshot.Output, 13),-15}");

                SetStatusColor(snapshot.Status);
                Console.Write($"{snapshot.Status,-12}");
                Console.ResetColor();

                Console.WriteLine(snapshot.ProgressLine);
            }

            Console.WriteLine(new string('-', 80));
            Console.WriteLine($"Total Jobs: {snapshots.Count} | Running: {runningCount} | Queued: {queuedCount}");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\n------------------------------------------------------------");
            Console.WriteLine("[Backspace] Back to Main Menu");
            Console.ResetColor();

            Thread.Sleep(300);
        }
    }

    protected override NavigationResult HandleOption(string option) => NavigationResult.Back();

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value.Length <= maxLength ? value : value[..(maxLength - 3)] + "...";
    }
    private static void SetStatusColor(JobStatus status)
    {
        Console.ForegroundColor = status switch
        {
            JobStatus.Running => ConsoleColor.Yellow,
            JobStatus.Completed => ConsoleColor.Green,
            JobStatus.Failed => ConsoleColor.Red,
            JobStatus.Canceled => ConsoleColor.DarkGray,
            _ => ConsoleColor.White
        };
    }
}