namespace MenuLib;

public struct Option
{
    public string Key { get; }
    public string Value { get; }

    public Option(string key, string value)
    {
        Key = key;
        Value = value;
    }
}

public abstract class Menu
{
    public string Title { get; }

    private Option[] _options;
    private int _optionIndex;

    public Menu(string title)
    {
        Title = title;
    }

    protected void ConfigureOptionSize(int count)
    {
        _options = new Option[count];
        _optionIndex = 0;
    }

    protected void AddOption(string key, string value)
    {
        _options[_optionIndex] = new Option(key, value);
        _optionIndex++;
    }

    private bool ContainsOption(string key)
    {
        foreach (var option in _options)
        {
            if (option.Key == key)
            {
                return true;
            }
        }

        return false;
    }

    public void Display()
    {
        Console.Clear();
        Console.WriteLine($"=== {Title} ===");

        if (_options != null)
        {
            foreach (var option in _options)
            {
                Console.WriteLine($"{option.Key} - {option.Value}");
            }
        }

        InternalDisplay();

        Console.WriteLine("\n--- Navigation ---");
        Console.WriteLine("Type 'refresh' to refresh.");
        Console.WriteLine("Type 'back' to go back.");
        Console.WriteLine("Type 'exit' to exit.");
    }

    protected virtual void InternalDisplay()
    {

    }
    
    protected void DrawMenu(int selectedIndex)
    {
        Console.Clear();
        Console.WriteLine($"=== {Title} ===");

        if (_options != null)
        {
            for (int i = 0; i < _options.Length; i++)
            {
                var option = _options[i];
                if (i == selectedIndex)
                {
                    Console.BackgroundColor = ConsoleColor.Magenta;
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"> {option.Value} <");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine($"  {option.Value}");
                }
            }
        }

        InternalDisplay();

        Console.WriteLine("\n--- Navigation ---");
        Console.WriteLine("Enter - select   Backspace - back   R - refresh   X - exit");
    }

    public virtual NavigationResult InteractiveSelect()
    {
        int selectedIndex = 0;

        while (true)
        {
            DrawMenu(selectedIndex);

            var key = Console.ReadKey(true).Key;

            if (key == ConsoleKey.UpArrow || key == ConsoleKey.W)
            {
                selectedIndex = (selectedIndex == 0) ? _options.Length - 1 : selectedIndex - 1;
            }
            else if (key == ConsoleKey.DownArrow || key == ConsoleKey.S)
            {
                selectedIndex = (selectedIndex + 1) % _options.Length;
            }
            else if (key == ConsoleKey.R)
            {
                return NavigationResult.Refresh();
            }
            else if (key == ConsoleKey.Backspace  || key == ConsoleKey.LeftArrow  || key == ConsoleKey.A)
            {
                return NavigationResult.Back();
            }
            else if (key == ConsoleKey.Enter|| key == ConsoleKey.Spacebar  || key == ConsoleKey.RightArrow  || key == ConsoleKey.D)
            {
                return HandleOption(_options[selectedIndex].Key);
            }
            else if (key == ConsoleKey.X)
            {
                return NavigationResult.Exit();
            }
        }
    }
    
    public NavigationResult ExecuteOption(string option)
    {
        if (option == "refresh")
        {
            return NavigationResult.Refresh();
        }
        
        if (option == "back")
        {
            return NavigationResult.Back();
        }

        if (option == "exit")
        {
            return NavigationResult.Exit();
        }

        if (ContainsOption(option))
        {
            Console.Clear();
            return HandleOption(option);
        }

        Console.WriteLine("Invalid option.");
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey(true);

        return NavigationResult.None();
    }

    protected abstract NavigationResult HandleOption(string option);
}