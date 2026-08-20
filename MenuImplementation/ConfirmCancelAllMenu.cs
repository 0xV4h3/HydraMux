namespace MenuImplementation;

public class ConfirmCancelAllMenu : Menu
{
    private readonly JobManager _manager;

    public ConfirmCancelAllMenu(JobManager manager) : base(MenuTitles.ConfirmCancelAllTitle)
    {
        _manager = manager;

        ConfigureOptionSize(2);
        AddOption("no", "No, keep jobs running");
        AddOption("yes", "Yes, cancel ALL running/queued jobs");
    }

    protected override void InternalDisplay()
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\n[WARNING] This action will immediately kill all active and queued tasks.");
        Console.ResetColor();
    }

    protected override NavigationResult HandleOption(string option)
    {
        if (option == "yes")
        {
            Console.Clear();
            Console.WriteLine("=== CANCEL ALL JOBS ===");
            _manager.CancelAll();
            Console.WriteLine("All running/queued jobs cancelled.");
            Thread.Sleep(1200);
        }
        
        return NavigationResult.Back();
    }
}