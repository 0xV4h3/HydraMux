using FFMpegCore;
using FFMpegCore.Extensions.Downloader;
using FFMpegCore.Extensions.Downloader.Enums;

namespace Core;

public static class FFmpegBootstrapper
{
    public static async Task<string> EnsureBinariesInstalledAsync()
    {
        string binariesFolder = Path.Combine(AppContext.BaseDirectory, "FFmpegBinaries");
        Directory.CreateDirectory(binariesFolder);

        bool isWindows = OperatingSystem.IsWindows();
        string ffmpegPath = Path.Combine(binariesFolder, isWindows ? "ffmpeg.exe" : "ffmpeg");
        string ffprobePath = Path.Combine(binariesFolder, isWindows ? "ffprobe.exe" : "ffprobe");

        if (!File.Exists(ffmpegPath) || !File.Exists(ffprobePath))
        {
            Console.WriteLine("FFmpeg not found. Downloading binaries for the current platform...");

            var options = new FFOptions { BinaryFolder = binariesFolder };
            await FFMpegDownloader.DownloadBinaries(
                version: FFMpegVersions.LatestAvailable,
                binaries: FFMpegBinaries.FFMpeg | FFMpegBinaries.FFProbe,
                options: options);

            Console.WriteLine("Done.");
        }

        GlobalFFOptions.Configure(o => o.BinaryFolder = binariesFolder);
        return binariesFolder;
    }
}