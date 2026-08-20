using System.Diagnostics;

namespace Core;

public enum JobStatus
{
    Queued, 
    Running,
    Completed,
    Failed,
    Canceled
}

public readonly record struct JobSnapshot(
    int Id,
    string Input,
    string Output,
    JobStatus Status,
    string ProgressLine);

public class Job
{
    public int Id { get; set; }
    public string Input { get; set; } = "";
    public string Output { get; set; } = "";
    public string Options { get; set; } = "";
    public JobStatus Status { get; set; } = JobStatus.Queued;

    public ConsoleProgressBar? ProgressBar { get; set; }
    public ulong CurrentTick { get; set; }
    public ulong TotalTicks { get; set; }
    public string CustomMessage { get; set; } = "";

    public Process? LiveProcess { get; set; }
}