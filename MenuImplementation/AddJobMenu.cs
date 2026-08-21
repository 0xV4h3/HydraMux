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
        
        Console.Write("Enter input path or file name: ");
        string input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input)) input = "video.mp4";

        Console.Write("Enter output path or file name (or leave empty to save near source): ");
        string output = Console.ReadLine() ?? "";

        Console.Write("Enter FFmpeg options (e.g., -c:v libx264 -crf 23): ");
        string options = Console.ReadLine() ?? "";
        
        try
        {
            Job job = _manager.AddJob(input, output, options);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nJob #{job.Id} added to queue successfully!");
            Console.ResetColor();
        }
        catch (FileNotFoundException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nError: {ex.Message}");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nUnexpected error: {ex.Message}");
            Console.ResetColor();
        }

        Console.WriteLine("\nPress any key to return...");
        Console.ReadKey(true);
    }

    protected override NavigationResult HandleOption(string option) => NavigationResult.Back();
}