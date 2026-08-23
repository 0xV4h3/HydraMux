using Core;
using MenuLib;
using MenuImplementation;

namespace ConversionManager;

class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        
        var (ffmpegPath, ffprobePath) = await FFmpegBootstrapper.EnsureBinariesInstalledAsync();
        
        Console.WriteLine($"FFmpeg is at: {ffmpegPath}");
        Console.WriteLine($"FFprobe is at: {ffprobePath}");

        var manager = new JobManager(ffmpegPath, ffprobePath);
        MenuRunner.Run(new AppMainMenu(manager));
    }
}