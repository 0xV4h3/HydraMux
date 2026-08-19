namespace ConversionManager;

public class JobManager(string workerExePath)
{
    private readonly string _workerExePath = workerExePath;
    private readonly List<Job> _jobs = new();
    private readonly object _lock = new(); // some locking primitive

    //TODO methods
}