namespace Core;

public class JobManager(string workerExePath)
{
    private readonly string _workerExePath = workerExePath;
    private readonly List<Job> _jobs = new();
    private readonly object _lock = new(); // some locking primitive

    //TODO methods

    public Job AddJob(string input, string output, string options)
    {
        throw new NotImplementedException();
    }
    
    private void Execute(Job job)
    {
        throw new NotImplementedException();
    }
    
    public bool CancelJob(int id)
    {
        throw new NotImplementedException();
    }
    
    public void CancelAll()
    {
        throw new NotImplementedException();
    }
    
    private void CancelInternal(Job job)
    {
        throw new NotImplementedException();
    }

    public ICollection<JobSnapshot> GetSnapshot()
    {
        throw new NotImplementedException();
    }
}