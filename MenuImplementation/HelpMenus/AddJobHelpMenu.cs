namespace MenuImplementation.HelpMenus;

public class AddJobHelpMenu : Menu
{
    public AddJobHelpMenu() : base(MenuTitles.AddJobHelpTitle) { ConfigureOptionSize(0); }
    
    protected override void InternalDisplay()
    {
        Console.WriteLine("Submits a new task to the background queue.");
        Console.WriteLine("You will be prompted to enter:");
        Console.WriteLine("  - Input path/name (defaults to 'video.mp4')");
        Console.WriteLine("  - Output path/name (defaults to 'out.mkv')");
        Console.WriteLine("  - Options text (extra arguments for the worker)");
    }

    protected override NavigationResult HandleOption(string option) => NavigationResult.None();
}