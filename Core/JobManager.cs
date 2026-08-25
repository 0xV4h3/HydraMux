using System.Diagnostics;
using System.Globalization;

namespace Core;

public class JobManager(string ffmpegPath, string ffprobePath)
{
    private readonly string _ffmpegPath = ffmpegPath;
    private readonly string _ffprobePath = ffprobePath;
    private readonly List<Job> _jobs = new();
    private readonly object _lock = new();
    private int _nextJobId = 1;

    private const int MaxStderrLinesKept = 20;

    public Job AddJob(string input, string output, string options)
    {
        string fullInput = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), input));

        if (!File.Exists(fullInput))
        {
            throw new FileNotFoundException($"Source file not found at path: {fullInput}");
        }

        string fullOutput = ResolveDestinationPath(fullInput, output);

        Job job;
        lock (_lock)
        {
            job = new Job
            {
                Id = _nextJobId++,
                Input = fullInput,
                Output = fullOutput,
                Options = options,
                Status = JobStatus.Queued
            };
            _jobs.Add(job);
        }

        ThreadPool.QueueUserWorkItem(_ => Execute(job));
        return job;
    }

    private void Execute(Job job)
    {
        lock (_lock)
        {
            if (job.Status == JobStatus.Canceled) return;
            job.Status = JobStatus.Running;
        }

        var stderrTail = new List<string>(MaxStderrLinesKept);
        var stderrLock = new object();

        void AppendStderrLine(string line)
        {
            lock (stderrLock)
            {
                stderrTail.Add(line);
                if (stderrTail.Count > MaxStderrLinesKept)
                    stderrTail.RemoveAt(0);
            }
        }

        try
        {
            ulong totalMilliseconds = GetVideoDurationMs(job.Input);
            if (totalMilliseconds == 0) totalMilliseconds = 1;

            long inputSizeBytes = new FileInfo(job.Input).Length;
            string inputSizeStr = FormatBytes(inputSizeBytes);

            lock (_lock)
            {
                job.TotalTicks = totalMilliseconds;
                job.ProgressBar = new ConsoleProgressBar(totalMilliseconds, 15, speed =>
                {
                    double speedRatio = speed / 1000.0;
                    return $"{speedRatio:F1}x";
                });
            }

            var psi = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = $"-progress pipe:1 -i \"{job.Input}\" {job.Options} -y \"{job.Output}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = new Process { StartInfo = psi, EnableRaisingEvents = true };

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data == null) return;

                if (e.Data.StartsWith("out_time_us="))
                {
                    string usStr = e.Data.Substring("out_time_us=".Length).Trim();
                    if (long.TryParse(usStr, out long microseconds) && microseconds > 0)
                    {
                        ulong currentMs = (ulong)(microseconds / 1000);

                        lock (_lock)
                        {
                            if (job.Status == JobStatus.Running)
                            {
                                job.CurrentTick = Math.Min(currentMs, job.TotalTicks);

                                long currentOutputSize = File.Exists(job.Output) ? new FileInfo(job.Output).Length : 0;
                                string outputSizeStr = FormatBytes(currentOutputSize);

                                job.CustomMessage = $"Out: {outputSizeStr} | Src: {inputSizeStr}";
                            }
                        }
                    }
                }
            };

            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data == null) return;
                AppendStderrLine(e.Data);
            };

            lock (_lock)
            {
                if (job.Status == JobStatus.Canceled) return;
                job.LiveProcess = process;
            }

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();

            lock (_lock)
            {
                if (job.Status == JobStatus.Running)
                {
                    if (process.ExitCode == 0)
                    {
                        job.Status = JobStatus.Completed;
                        job.ProgressBar?.ForceComplete();

                        long finalSize = File.Exists(job.Output) ? new FileInfo(job.Output).Length : 0;
                        job.CustomMessage = $"Final: {FormatBytes(finalSize)} | Src: {inputSizeStr}";
                    }
                    else
                    {
                        job.Status = JobStatus.Failed;
                        job.CustomMessage = BuildFailureMessage(process.ExitCode, stderrTail, stderrLock);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            lock (_lock)
            {
                if (job.Status != JobStatus.Canceled)
                {
                    job.Status = JobStatus.Failed;
                    job.CustomMessage = ex.Message;
                }
            }
        }
        finally
        {
            lock (_lock) { job.LiveProcess = null; }
        }
    }

    private static string BuildFailureMessage(int exitCode, List<string> stderrTail, object stderrLock)
    {
        string lastLine;
        lock (stderrLock)
        {
            lastLine = stderrTail.LastOrDefault(l => !string.IsNullOrWhiteSpace(l)) ?? "no stderr output captured";
        }

        const int maxLen = 160;
        if (lastLine.Length > maxLen)
            lastLine = lastLine.Substring(0, maxLen) + "...";

        return $"Error (Code: {exitCode}): {lastLine}";
    }

    private ulong GetVideoDurationMs(string inputPath)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = _ffprobePath,
                Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{inputPath}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null) return 0;

            string output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            if (double.TryParse(output, CultureInfo.InvariantCulture, out double seconds))
            {
                return (ulong)(seconds * 1000.0);
            }
        }
        catch { }
        return 0;
    }

    private static string ResolveDestinationPath(string sourcePath, string destinationPath)
    {
        string sourceFileName = Path.GetFileNameWithoutExtension(sourcePath);
        string sourceExtension = Path.GetExtension(sourcePath);

        if (string.IsNullOrWhiteSpace(destinationPath))
        {
            destinationPath = Path.GetDirectoryName(sourcePath) ?? Directory.GetCurrentDirectory();
        }
        else
        {
            destinationPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), destinationPath));
        }

        bool endsWithSlash = destinationPath.EndsWith(Path.DirectorySeparatorChar) ||
                             destinationPath.EndsWith(Path.AltDirectorySeparatorChar);

        bool isDirectoryTarget = endsWithSlash || Directory.Exists(destinationPath);

        string targetDirectory;
        string baseName;
        string extension;

        if (isDirectoryTarget)
        {
            targetDirectory = destinationPath;
            baseName = $"{sourceFileName}_converted";
            extension = sourceExtension;
        }
        else
        {
            targetDirectory = Path.GetDirectoryName(destinationPath) ?? Directory.GetCurrentDirectory();
            baseName = Path.GetFileNameWithoutExtension(destinationPath);
            extension = Path.GetExtension(destinationPath);

            if (string.IsNullOrWhiteSpace(extension))
                extension = sourceExtension;
        }

        Directory.CreateDirectory(targetDirectory);

        string candidate = Path.Combine(targetDirectory, $"{baseName}{extension}");
        int counter = 1;

        while (File.Exists(candidate))
        {
            candidate = Path.Combine(targetDirectory, $"{baseName}({counter}){extension}");
            counter++;
        }

        return candidate;
    }

    private static string FormatBytes(double bytes)
    {
        string[] suffix = { "B", "KB", "MB", "GB" };
        int i = 0;
        while (bytes >= 1024 && i < suffix.Length - 1) { bytes /= 1024; i++; }
        return $"{bytes:F1} {suffix[i]}";
    }

    public bool CancelJob(int jobId)
    {
        lock (_lock)
        {
            var job = _jobs.FirstOrDefault(j => j.Id == jobId);
            if (job == null)
                return false;

            return TryCancelJobInternal(job);
        }
    }

    public void CancelAll()
    {
        lock (_lock)
        {
            foreach (var job in _jobs)
            {
                TryCancelJobInternal(job);
            }

            Monitor.PulseAll(_lock);
        }
    }

    private bool TryCancelJobInternal(Job job)
    {
        if (job.Status != JobStatus.Queued && job.Status != JobStatus.Running)
        {
            return false;
        }

        job.Status = JobStatus.Canceled;
        job.CustomMessage = "Canceled.";
        job.ProgressBar?.Dispose();

        if (job.LiveProcess != null)
        {
            TryKillProcess(job.LiveProcess);
            job.LiveProcess = null;
        }

        Monitor.PulseAll(_lock);

        return true;
    }

    private static void TryKillProcess(Process process)
    {
        if (process == null)
            return;
        try
        {
            process.Kill(entireProcessTree: true);
        }
        catch (Exception ex) when (ex is InvalidOperationException
                               || ex is System.ComponentModel.Win32Exception)
        { }
        catch (AggregateException aggEx)
        {
            aggEx.Handle(inner => inner is InvalidOperationException
                               || inner is System.ComponentModel.Win32Exception);
        }
    }

    public ICollection<JobSnapshot> GetSnapshot()
    {
        lock (_lock)
        {
            return _jobs.Select(j => new JobSnapshot(
                j.Id,
                j.Input,
                j.Output,
                j.Status,
                j.Status == JobStatus.Running
                    ? j.ProgressBar?.GetProgressString(j.CurrentTick, j.CustomMessage) ?? j.CustomMessage
                    : j.ProgressBar?.GetLastString() ?? j.CustomMessage
            )).ToList();
        }
    }
}