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
                _selectedIndex = 0;
                Console.WriteLine("\n [ No active or queued jobs available ]                       \n");
            }
            else
            {
                _selectedIndex = Math.Clamp(_selectedIndex, 0, snapshots.Count - 1);

                int activeCount = snapshots.Count(s => s.Status == JobStatus.Running || s.Status == JobStatus.Queued);
                Console.WriteLine($"\n Total Jobs: {snapshots.Count} | Active: {activeCount}\n");

                for (int i = 0; i < snapshots.Count; i++)
                {
                    var snap = snapshots[i];
                    bool isSelected = (i == _selectedIndex);

                    if (isSelected)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(" > ");
                    }
                    else
                    {
                        Console.Write("   ");
                    }

                    Console.Write($"[{snap.Id:D3}] ");

                    SetStatusColor(snap.Status);
                    Console.Write($"[{snap.Status,-9}] ");
                    Console.ResetColor();

                    if (isSelected) Console.BackgroundColor = ConsoleColor.DarkGray;

                    string displayLine = string.IsNullOrWhiteSpace(snap.ProgressLine)
                        ? $"{snap.Input} -> {snap.Output}"
                        : snap.ProgressLine;

                    if (displayLine.Length > 48)
                        displayLine = displayLine[..45] + "...";

                    Console.WriteLine(displayLine.PadRight(50));
                    Console.ResetColor();
                }
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