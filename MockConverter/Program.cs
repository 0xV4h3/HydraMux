namespace MockConverter;

class Program
{
    static void Main(string[] args)
    {
        ulong totalBytes = 1024 * 1024 * 1024;
        ulong bufferSize = 4 * 1024 * 1024;
        ulong currentPosition = 0;

        Random rand = new Random();
        
        Console.WriteLine($"TOTAL:{totalBytes}");

        while (currentPosition < totalBytes)
        {
            Thread.Sleep(rand.Next(50, 150));
            currentPosition += bufferSize;
            
            Console.WriteLine($"TICK:{currentPosition}");
        }
        
        Environment.Exit(rand.Next(0, 10) == 0 ? 1 : 0);
    }
}