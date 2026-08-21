using Core;
using MenuLib;
using MenuImplementation;

namespace ConversionManager;

class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        
        string ffmpegFolder = await FFmpegBootstrapper.EnsureBinariesInstalledAsync();
        
        bool isWindows = OperatingSystem.IsWindows();
        string ffmpegPath = Path.Combine(ffmpegFolder, isWindows ? "ffmpeg.exe" : "ffmpeg");
        string ffprobePath = Path.Combine(ffmpegFolder, isWindows ? "ffprobe.exe" : "ffprobe");

        var manager = new JobManager(ffmpegPath, ffprobePath);
        MenuRunner.Run(new AppMainMenu(manager));
    }
}