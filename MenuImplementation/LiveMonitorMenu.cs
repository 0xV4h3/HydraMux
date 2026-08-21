namespace MenuImplementation;

public class LiveMonitorMenu : Menu
{
    private readonly JobManager _manager;
    
    private const int IdWidth = 4;
    private const int InputWidth = 20;
    private const int OutputWidth = 20;
    private const int StatusWidth = 10;
    
    private const int HeaderColumnsWidth = IdWidth + InputWidth + OutputWidth + StatusWidth + 4;
    
    private const string DefaultWaitingMetrics = "[░░░░░░░░░░░░░░░]   0.0% | Waiting...";
    private static readonly int DefaultMetricsLength = DefaultWaitingMetrics.Length;

    private const string NoJobsMessage = " [ No active or queued jobs available ] ";
    private const int StaticPartLength = IdWidth + InputWidth + OutputWidth + StatusWidth + 4;

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
                Console.WriteLine(NoJobsMessage.PadRight(Console.WindowWidth - 1));
                currentLineCount++;
            }
            else
            {
                int maxMetricsLength = snapshots.Max(s => string.IsNullOrWhiteSpace(s.ProgressLine) 
                    ? DefaultMetricsLength 
                    : s.ProgressLine.Length);
                
                int tableLineWidth = HeaderColumnsWidth + maxMetricsLength;
                
                string headerLine = $"{"ID",-IdWidth} {"Input File",-InputWidth} {"Output File",-OutputWidth} {"Status",-StatusWidth} Progress & Metrics";
                Console.WriteLine(headerLine.PadRight(Console.WindowWidth - 1));
                
                string topSeparator = new string('-', tableLineWidth);
                Console.WriteLine(topSeparator.PadRight(Console.WindowWidth - 1));
                currentLineCount += 2;

                int runningCount = 0;
                int queuedCount = 0;

                foreach (var snap in snapshots)
                {
                    if (snap.Status == JobStatus.Running) runningCount++;
                    else if (snap.Status == JobStatus.Queued) queuedCount++;

                    string inputName = TruncateWithEllipsis(Path.GetFileName(snap.Input), InputWidth - 1);
                    string outputName = TruncateWithEllipsis(Path.GetFileName(snap.Output), OutputWidth - 1);

                    string rowStart = $"{snap.Id,-IdWidth} {inputName,-InputWidth} {outputName,-OutputWidth} ";
                    Console.Write(rowStart);

                    SetStatusColor(snap.Status);
                    Console.Write($"{snap.Status.ToString(),-StatusWidth} ");
                    Console.ResetColor();

                    string metrics = string.IsNullOrWhiteSpace(snap.ProgressLine)
                        ? DefaultWaitingMetrics
                        : snap.ProgressLine;

                    int remainingWidth = Console.WindowWidth - StaticPartLength - 1;
                    
                    if (remainingWidth > 0)
                        Console.WriteLine(metrics.PadRight(remainingWidth));
                    else
                        Console.WriteLine(metrics);
                    
                    currentLineCount++;
                }

                string bottomSeparator = new string('-', tableLineWidth);
                Console.WriteLine(bottomSeparator.PadRight(Console.WindowWidth - 1));
                
                string totalLine = $"Total Jobs: {snapshots.Count} | Running: {runningCount} | Queued: {queuedCount}";
                Console.WriteLine(totalLine.PadRight(Console.WindowWidth - 1));
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

    private static string TruncateWithEllipsis(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength) return text;
        return text.Substring(0, maxLength - 3) + "...";
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