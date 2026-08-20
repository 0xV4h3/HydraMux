namespace MenuImplementation;

public class LiveMonitorMenu : Menu
{
    private readonly JobManager _manager;
    private int _selectedIndex = 0;
    private bool _isPaused = false;

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

            var snapshots = _manager.GetSnapshot().ToList();

            if (snapshots.Count == 0)
            {
                Console.WriteLine(" [ No active or queued jobs available ] ");
            }
            else
            {
                Console.WriteLine($"{"ID",-4} {"Input",-14} {"Output",-14} {"Status",-10} Progress & Metrics");
                Console.WriteLine(new string('-', 85));

                int runningCount = 0;
                int queuedCount = 0;

                foreach (var snap in snapshots)
                {
                    if (snap.Status == JobStatus.Running) runningCount++;
                    else if (snap.Status == JobStatus.Queued) queuedCount++;

                    Console.Write($"{snap.Id,-4} {snap.Input,-14} {snap.Output,-14} ");

                    SetStatusColor(snap.Status);
                    Console.Write($"{snap.Status.ToString(),-10}");
                    Console.ResetColor();

                    string metrics = string.IsNullOrWhiteSpace(snap.ProgressLine)
                        ? "[░░░░░░░░░░░░░░░]   0.0% | Waiting..."
                        : snap.ProgressLine;

                    Console.WriteLine(metrics);
                }

                Console.WriteLine(new string('-', 85));
                Console.WriteLine($"Total Jobs: {snapshots.Count} | Running: {runningCount} | Queued: {queuedCount}");
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\n------------------------------------------------------------");
            Console.WriteLine("[Backspace] Back to Main Menu");
            Console.ResetColor();

            Thread.Sleep(300);
        }
    }
    
    protected override NavigationResult HandleOption(string option) => NavigationResult.Back();
    
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