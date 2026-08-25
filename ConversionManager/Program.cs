using Core;
using MenuLib;
using MenuImplementation;

namespace ConversionManager;

class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        
        var (success, ffDirs) = await FFmpegBootstrapper.TryEnsureBinariesInstalledAsync();

        if (!success || ffDirs is not { } validDirs)
        {
            Console.WriteLine("Failed to find or install FFMpeg binaries");
            return;
        }

        var (ffmpegPath, ffprobePath) = validDirs;

        Console.WriteLine($"FFmpeg is at: {ffmpegPath}");
        Console.WriteLine($"FFprobe is at: {ffprobePath}");

        var manager = new JobManager(ffmpegPath, ffprobePath);
        MenuRunner.Run(new AppMainMenu(manager));
    }
}