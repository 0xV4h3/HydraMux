namespace MenuImplementation.HelpMenus;

public class ConcurrencyHelpMenu : Menu
{
    public ConcurrencyHelpMenu() : base(MenuTitles.ConcurrencyHelpTitle) { ConfigureOptionSize(0); }

    protected override void InternalDisplay()
    {
        Console.WriteLine("All conversion tasks run asynchronously via ThreadPool.");
        Console.WriteLine("The UI loop operates independently and never blocks job execution.");
        Console.WriteLine("Exiting the manager will abort all active conversions.");
    }

    protected override NavigationResult HandleOption(string option) => NavigationResult.None();
}