using FFMpegCore;
using FFMpegCore.Extensions.Downloader;
using FFMpegCore.Extensions.Downloader.Enums;
using System.Runtime.Versioning;

namespace Core;

public static class FFmpegBootstrapper
{
    public static async Task<(bool Success, FFDirs? Dirs)> TryEnsureBinariesInstalledAsync()
    {
        try
        {
            FFDirs ffDirs = await EnsureBinariesInstalledAsync();
            
            if (!ffDirs.IsValid) 
            {
                return (false, null);
            }

            return (true, ffDirs);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to check FFMpeg binaries {e.Message}");
            return (false, null);
        }
    }
    
    public static async Task<FFDirs> EnsureBinariesInstalledAsync()
    {
        bool isWindows = OperatingSystem.IsWindows();
        string ffmpegExecutable = isWindows ? "ffmpeg.exe" : "ffmpeg";
        string ffprobeExecutable = isWindows ? "ffprobe.exe" : "ffprobe";

        string? systemFFmpeg = FindExecutable(ffmpegExecutable);
        string? systemFFprobe = FindExecutable(ffprobeExecutable);

        if (systemFFmpeg != null && systemFFprobe != null)
        {
            ConfigureBinaryFolderIfShared(systemFFmpeg, systemFFprobe);

            Console.WriteLine($"Using system ffmpeg from: {systemFFmpeg}");
            Console.WriteLine($"Using system ffprobe from: {systemFFprobe}");

            return new FFDirs(systemFFmpeg, systemFFprobe);
        }

        string binariesFolderName = "FFmpegBinaries";
        string binariesFolder = Path.Combine(AppContext.BaseDirectory, binariesFolderName);
        Directory.CreateDirectory(binariesFolder);

        string localFFmpegPath = Path.Combine(binariesFolder, ffmpegExecutable);
        string localFFprobePath = Path.Combine(binariesFolder, ffprobeExecutable);

        if (!File.Exists(localFFmpegPath) || !File.Exists(localFFprobePath))
        {
            Console.WriteLine("FFmpeg not found in system PATH. Downloading binaries for the current platform...");

            var options = new FFOptions { BinaryFolder = binariesFolder };

            try
            {
                await FFMpegDownloader.DownloadBinaries(
                    version: FFMpegVersions.LatestAvailable,
                    binaries: FFMpegBinaries.FFMpeg | FFMpegBinaries.FFProbe,
                    options: options);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to download binaries {e.Message}");
                throw;
            }

            if (!isWindows)
            {
                SetLinuxExecutePermission(localFFmpegPath);
                SetLinuxExecutePermission(localFFprobePath);
            }

            Console.WriteLine("Download completed successfully.");
        }

        GlobalFFOptions.Configure(o => o.BinaryFolder = binariesFolder);
        Console.WriteLine($"Using local binaries from: {binariesFolder}");

        return new FFDirs(localFFmpegPath, localFFprobePath);
    }

    private static void ConfigureBinaryFolderIfShared(string ffmpegPath, string ffprobePath)
    {
        string? ffmpegDir = Path.GetDirectoryName(ffmpegPath);
        string? ffprobeDir = Path.GetDirectoryName(ffprobePath);

        if (ffmpegDir != null && string.Equals(ffmpegDir, ffprobeDir, StringComparison.OrdinalIgnoreCase))
        {
            GlobalFFOptions.Configure(o => o.BinaryFolder = ffmpegDir);
        }
    }

    private static string? FindExecutable(string executableName)
    {
        string localPath = Path.Combine(AppContext.BaseDirectory, executableName);
        if (File.Exists(localPath)) return localPath;

        string? pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathEnv)) return null;

        char pathSeparator = Path.PathSeparator;
        string[] paths = pathEnv.Split(pathSeparator, StringSplitOptions.RemoveEmptyEntries);

        foreach (string path in paths)
        {
            try
            {
                string fullPath = Path.Combine(path.Trim(), executableName);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }
            catch (ArgumentException) { }
        }

        return null;
    }

    private static void SetLinuxExecutePermission(string filePath)
    {
        if (OperatingSystem.IsWindows())
            return;

        try
        {
            if (File.Exists(filePath))
            {
                ApplyUnixPermissions(filePath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to set execute permissions (chmod +x) for {filePath}. Error: {ex.Message}");
        }
    }

    [UnsupportedOSPlatform("windows")]
    private static void ApplyUnixPermissions(string filePath)
    {
        var currentMode = File.GetUnixFileMode(filePath);
        var newMode = currentMode | UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute;
        File.SetUnixFileMode(filePath, newMode);
    }
}