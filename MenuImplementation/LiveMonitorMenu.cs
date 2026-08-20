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

            // TODO Logic
            
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