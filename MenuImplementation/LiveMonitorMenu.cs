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
        
        int lastLineCount = 0; 

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
            int currentLineCount = 0;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("=== LIVE MONITOR ===\n");
            Console.ResetColor();
            currentLineCount += 2;

            var snapshots = _manager.GetSnapshot().ToList();

            if (snapshots.Count == 0)
            {
                Console.WriteLine(" [ No active or queued jobs available ] ".PadRight(120));
                currentLineCount++;
            }
            else
            {
                Console.WriteLine($"{"ID",-4} {"Input File",-20} {"Output File",-20} {"Status",-10} Progress & Metrics");
                Console.WriteLine(new string('-', 110));
                currentLineCount += 2;

                int runningCount = 0;
                int queuedCount = 0;

                foreach (var snap in snapshots)
                {
                    if (snap.Status == JobStatus.Running) runningCount++;
                    else if (snap.Status == JobStatus.Queued) queuedCount++;

                    string inputName = Path.GetFileName(snap.Input);
                    string outputName = Path.GetFileName(snap.Output);
                    
                    if (inputName.Length > 19) inputName = inputName.Substring(0, 16) + "...";
                    if (outputName.Length > 19) outputName = outputName.Substring(0, 16) + "...";

                    Console.Write($"{snap.Id,-4} {inputName,-20} {outputName,-20} ");

                    SetStatusColor(snap.Status);
                    Console.Write($"{snap.Status.ToString(),-10}");
                    Console.ResetColor();

                    string metrics = string.IsNullOrWhiteSpace(snap.ProgressLine)
                        ? "[░░░░░░░░░░░░░░░]   0.0% | Waiting..."
                        : snap.ProgressLine;

                    Console.WriteLine(metrics.PadRight(80));
                    currentLineCount++;
                }

                Console.WriteLine(new string('-', 110));
                
                string totalLine = $"Total Jobs: {snapshots.Count} | Running: {runningCount} | Queued: {queuedCount}";
                Console.WriteLine(totalLine.PadRight(110));
                currentLineCount += 2;
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\n------------------------------------------------------------");
            Console.WriteLine("[Backspace] Back to Main Menu");
            Console.ResetColor();
            currentLineCount += 3;
            
            if (currentLineCount < lastLineCount)
            {
                string blankLine = new string(' ', 120);
                for (int i = currentLineCount; i < lastLineCount; i++)
                {
                    Console.WriteLine(blankLine);
                }
            }
            lastLineCount = currentLineCount;

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