namespace MenuLib;

public class NavigationResult
{
    public NavigationResultType Type { get; }
    public Menu? Menu { get; }
    public string? Title { get; }

    private NavigationResult(NavigationResultType type, Menu? menu = null, string? title = null)
    {
        Type = type;
        Menu = menu;
        Title = title;
    }

    public static NavigationResult None()
    {
        return new NavigationResult(NavigationResultType.None);
    }


    public static NavigationResult GoTo(Menu next)
    {
        return new NavigationResult(NavigationResultType.GoTo, menu:next);
    }

    public static NavigationResult Wait()
    {
        return new NavigationResult(NavigationResultType.Wait);
    }

    public static NavigationResult Back()
    {
        return new NavigationResult(NavigationResultType.Back);
    }
    
    public static NavigationResult Jump(string title)
    {
        return new NavigationResult(NavigationResultType.Jump, title:title);
    }
    
    public static NavigationResult ToRoot()
    {
        return new NavigationResult(NavigationResultType.ToRoot);
    }

    public static NavigationResult Exit()
    {
        return new NavigationResult(NavigationResultType.Exit);
    }
}