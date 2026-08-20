namespace MenuLib;

public class MenuStack
{
    private Menu[] _items;
    private int _count;

    public int Count => _count;

    public MenuStack(int capacity=2)
    {
        _items = new Menu[capacity];
        _count = 0;
    }
    
 
    public void Push(Menu item)
    {
        if (_count == _items.Length)
        {
            Resize();
        }

        _items[_count] = item;
        _count++;
    }

    public Menu Pop()
    {
        if (_count==0)
        {
            throw new InvalidOperationException("Stack is empty");
        }
        
        _count--;
        return _items[_count];
    }
    
    public Menu Peek()
    {
        if (_count == 0)
        {
            throw new InvalidOperationException("Stack is empty");
        }

        return _items[_count - 1];
    }

    private void Resize()
    {
        Menu[] newItems = new Menu[_items.Length * 2];
        for (int i = 0; i < _items.Length; i++)
        {
            newItems[i] = _items[i];
        }

        _items = newItems;
    }
}