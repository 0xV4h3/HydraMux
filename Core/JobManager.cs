using System.Diagnostics;

namespace Core;

public class JobManager(string workerExePath)
{
    private readonly string _workerExePath = workerExePath;
    private readonly List<Job> _jobs = new();
    private readonly object _lock = new();
    private int _nextJobId = 1;

    public Job AddJob(string input, string output, string options)
    {
        Job job;
        lock (_lock)
        {
            job = new Job
            {
                Id = _nextJobId++,
                Input = input,
                Output = output,
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
        throw new NotImplementedException();
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