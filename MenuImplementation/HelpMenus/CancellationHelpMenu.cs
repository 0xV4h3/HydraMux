namespace MenuImplementation.HelpMenus;

public class CancellationHelpMenu : Menu
{
    public CancellationHelpMenu() : base(MenuTitles.CancellationHelpTitle) { ConfigureOptionSize(0); }

    protected override void InternalDisplay()
    {
        Console.WriteLine("Cancel One Job:");
        Console.WriteLine("  - Requires you to enter a valid integer Job ID.");
        Console.WriteLine("  - Displays 'Job not found' if the ID does not exist.");
        Console.WriteLine("\nCancel All Jobs:");
        Console.WriteLine("  - Instantly stops all active and queued tasks at once.");
    }

    protected override NavigationResult HandleOption(string option) => NavigationResult.None();
}