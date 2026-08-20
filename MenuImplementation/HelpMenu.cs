using MenuImplementation.HelpMenus;

namespace MenuImplementation;

public class HelpMenu : Menu
{
    public HelpMenu() : base(MenuTitles.HelpSystemTitle)
    {
        ConfigureOptionSize(4);
        AddOption("help_add", "How to Add Jobs");
        AddOption("help_monitor", "How Live Monitor works");
        AddOption("help_cancel", "How to Cancel Tasks");
        AddOption("help_concurrency", "Concurrency & Architecture");
    }

    protected override NavigationResult HandleOption(string option)
    {
        switch (option)
        {
            case "help_add": return NavigationResult.GoTo(new AddJobHelpMenu());
            case "help_monitor": return NavigationResult.GoTo(new LiveMonitorHelpMenu());
            case "help_cancel": return NavigationResult.GoTo(new CancellationHelpMenu());
            case "help_concurrency": return NavigationResult.GoTo(new ConcurrencyHelpMenu());
        }
        return NavigationResult.None();
    }
}