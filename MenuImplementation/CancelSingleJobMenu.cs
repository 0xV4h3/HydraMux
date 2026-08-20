namespace MenuImplementation;

public class CancelSingleJobMenu : Menu
{
    private readonly JobManager _manager;

    public CancelSingleJobMenu(JobManager manager) : base(MenuTitles.CancelSingleJobTitle)
    {
        _manager = manager;
        
        ConfigureOptionSize(0);
    }

    protected override void InternalDisplay()
    {
        Console.CursorVisible = true;
        
        Console.Write("Enter Job ID to cancel: ");

        if (int.TryParse(Console.ReadLine(), out int id))
        {
            if (_manager.CancelJob(id))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Job #{id} cancellation signal sent.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Job not found.");
            }
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Invalid ID format. Please enter a number.");
        }
        Console.ResetColor();
    }

    protected override NavigationResult HandleOption(string option) => NavigationResult.Back();
}