namespace MenuImplementation;

public class AppMainMenu : Menu
{
    private readonly JobManager _manager;

    public AppMainMenu(JobManager manager) : base(MenuTitles.ConversionManagerTitle)
    {
        _manager = manager;

        ConfigureOptionSize(5);
        AddOption("add", "Add Job");
        AddOption("monitor", "Monitor Progress (Live)");
        AddOption("cancel_one", "Cancel One Job");
        AddOption("cancel_all", "Cancel All Jobs");
        AddOption("help", "Help Screen");
    }

    protected override NavigationResult HandleOption(string option)
    {
        switch (option)
        {
            case "add":
                return NavigationResult.GoTo(new AddJobMenu(_manager));
            case "monitor":
                return NavigationResult.GoTo(new LiveMonitorMenu(_manager));
            case "cancel_one":
                return NavigationResult.GoTo(new CancelSingleJobMenu(_manager));
            case "cancel_all":
                return NavigationResult.GoTo(new ConfirmCancelAllMenu(_manager));
            case "help":
                return NavigationResult.GoTo(new HelpMenu());
        }

        return NavigationResult.None();
    }
}