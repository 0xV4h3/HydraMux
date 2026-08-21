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
        // lock (_lock)
        // {
        //     if (job.Status == JobStatus.Canceled) return;
        //     job.Status = JobStatus.Running;
        // }
        //
        // if (!File.Exists(_workerExePath))
        // {
        //     lock (_lock) { job.Status = JobStatus.Failed; }
        //     return;
        // }
        //
        // try
        // {
        //     var psi = new ProcessStartInfo
        //     {
        //         FileName = _workerExePath,
        //         Arguments = $"\"{job.Input}\" \"{job.Output}\" {job.Options}",
        //         RedirectStandardOutput = true,
        //         UseShellExecute = false,
        //         CreateNoWindow = true
        //     };
        //
        //     var process = new Process { StartInfo = psi };
        //
        //     lock (_lock)
        //     {
        //         if (job.Status == JobStatus.Canceled) return;
        //         job.LiveProcess = process;
        //     }
        //
        //     process.Start();
        //
        //     using (StreamReader reader = process.StandardOutput)
        //     {
        //         string? line;
        //         while ((line = reader.ReadLine()) != null)
        //         {
        //             if (line.StartsWith("TOTAL:"))
        //             {
        //                 if (ulong.TryParse(line.Substring(6), out ulong total))
        //                 {
        //                     lock (_lock)
        //                     {
        //                         job.TotalTicks = total;
        //                         job.ProgressBar = new ConsoleProgressBar(total);
        //                     }
        //                 }
        //             }
        //             else if (line.StartsWith("TICK:"))
        //             {
        //                 if (ulong.TryParse(line.Substring(5), out ulong current))
        //                 {
        //                     lock (_lock)
        //                     {
        //                         if (job.Status == JobStatus.Running)
        //                         {
        //                             job.CurrentTick = current;
        //                             job.CustomMessage = $"{FormatBytes(current)} / {FormatBytes(job.TotalTicks)}";
        //                         }
        //                     }
        //                 }
        //             }
        //         }
        //     }
        //
        //     process.WaitForExit();
        //
        //     lock (_lock)
        //     {
        //         if (job.Status == JobStatus.Running)
        //         {
        //             if (process.ExitCode == 0)
        //             {
        //                 job.Status = JobStatus.Completed;
        //                 job.ProgressBar?.ForceComplete();
        //             }
        //             else
        //             {
        //                 job.Status = JobStatus.Failed;
        //             }
        //         }
        //     }
        // }
        // catch
        // {
        //     lock (_lock)
        //     {
        //         if (job.Status != JobStatus.Canceled) job.Status = JobStatus.Failed;
        //     }
        // }
        // finally
        // {
        //     lock (_lock) { job.LiveProcess = null; }
        // }
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