namespace MenuLib;

public static class MenuRunner
{
    private static readonly MenuStack _navigationStack = new MenuStack();
    public static void Run(Menu root)
    {
        _navigationStack.Push(root);

        while (_navigationStack.Count > 0)
        {
            var currentMenu = _navigationStack.Peek();
            
            var result = currentMenu.InteractiveSelect();
            
            switch (result.Type)
            {
                case NavigationResultType.None:
                    break;
                case NavigationResultType.GoTo:
                    _navigationStack.Push(result.Menu);
                    break;
                case NavigationResultType.Wait:
                    Console.WriteLine("Waiting..., Press any key to continue...");
                    Console.ReadKey(true);
                    break;
                case NavigationResultType.Refresh:
                    Type menuType = currentMenu.GetType();
                    if (typeof(Menu).IsAssignableFrom(menuType))
                    {
                        _navigationStack.Pop();
                        _navigationStack.Push((Menu)Activator.CreateInstance(menuType));
                    }
                    else 
                    {
                        throw new InvalidOperationException(
                            $"Failed to refresh the menu. Type '{menuType.FullName}' does not inherit from the Menu class.");
                    }
                    break;
                case NavigationResultType.Back:
                    if (_navigationStack.Count > 1) _navigationStack.Pop();
                    break;
                case NavigationResultType.Jump:
                    MenuStack tempStack = new MenuStack();
                    bool menuFound = false;
                    
                    while (_navigationStack.Count != 0)
                    {
                        if (_navigationStack.Peek().Title == result.Title)
                        {
                            menuFound = true;
                            break;
                        }
                        tempStack.Push(_navigationStack.Pop());
                    }
                    
                    if (!menuFound)
                    {
                        while (tempStack.Count != 0)
                        {
                            _navigationStack.Push(tempStack.Pop());
                        }
                    }
                    break;
                case NavigationResultType.ToRoot:
                    while (_navigationStack.Count != 1) _navigationStack.Pop();
                    break;
                case NavigationResultType.Exit:
                    return;
            }
        }
    }
}