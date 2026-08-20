namespace MenuImplementation.HelpMenus;

public class LiveMonitorHelpMenu : Menu
{
    public LiveMonitorHelpMenu() : base(MenuTitles.LiveMonitorHelpTitle) { ConfigureOptionSize(0); }

    protected override void InternalDisplay()
    {
        Console.WriteLine("Renders a live snapshot of all jobs and their metrics.");
        Console.WriteLine("Refreshes automatically every 300ms without flickering.");
        Console.WriteLine("\nExample output:");
        Console.WriteLine("----------------------------------------------------------------------------------------------------");
        Console.WriteLine("ID   Input        Output       Status       Progress & Metrics");
        Console.WriteLine("----------------------------------------------------------------------------------------------------");
        
        Console.Write("1    movie.mp4    out_1.mkv    ");
        Console.ForegroundColor = ConsoleColor.Yellow; 
        Console.Write("Running      ");
        Console.ResetColor();
        Console.WriteLine("[████████░░░░░░░] 53.3% | 24.1 MB/s | ETA: 00:06 | 160.0 MB / 300.0 MB");
        
        Console.Write("2    clip.avi     out_2.mp4    ");
        Console.ForegroundColor = ConsoleColor.White;  
        Console.Write("Queued       ");
        Console.ResetColor();
        Console.WriteLine("[░░░░░░░░░░░░░░░] 0.0% | Waiting...");
        
        Console.WriteLine("----------------------------------------------------------------------------------------------------");
        Console.WriteLine("Total Jobs: 2 | Running: 1 | Queued: 1");
        Console.WriteLine("\nPress [Backspace] key while inside the monitor to return to the main menu.");
    }

    protected override NavigationResult HandleOption(string option) => NavigationResult.None();
}