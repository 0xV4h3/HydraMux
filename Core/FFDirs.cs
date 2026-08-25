namespace Core;

public readonly struct FFDirs(string ffmpegDir, string ffprobeDir)
{
    public string FFMpegDir { get; init; } = ffmpegDir;
    public string FFProbeDir { get; init; } = ffprobeDir;
    public bool IsValid => !string.IsNullOrEmpty(FFMpegDir) && !string.IsNullOrEmpty(FFProbeDir);
    public void Deconstruct(out string ffmpegDir, out string ffprobeDir)
    {
        ffmpegDir = FFMpegDir;
        ffprobeDir = FFProbeDir;
    }
}