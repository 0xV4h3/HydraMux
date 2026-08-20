namespace MenuImplementation;

public class AddJobMenu : Menu
{
    private readonly JobManager _manager;

    public AddJobMenu(JobManager manager) : base(MenuTitles.AddJobTitle) 
    {
        _manager = manager;
        
        ConfigureOptionSize(0);
    }

    protected override void InternalDisplay()
    {
        Console.CursorVisible = true;
        
        Console.Write("Enter input path/name: ");
        string input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input)) input = "video.mp4";

        Console.Write("Enter output path/name: ");
        string output = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(output)) output = "out.mkv";

        Console.Write("Enter options text: ");
        string options = Console.ReadLine() ?? "";
        
        Job job = _manager.AddJob(input, output, options);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\nJob #{job.Id} added to queue successfully!");
        Console.ResetColor();
    }

    protected override NavigationResult HandleOption(string option) => NavigationResult.Back();
}