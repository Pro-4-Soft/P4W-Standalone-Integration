namespace Pro4Soft.BackgroundWorker.Execution;

public class EntryPoint
{
    public async Task Startup()
    {
        await Start();
        await Console.In.ReadLineAsync();
        Stop();
    }

    protected virtual Task Start()
    {
        ScheduleThread.Instance.Start();
        return Task.CompletedTask;
    }

    protected virtual void Stop()
    {
        ScheduleThread.Instance.Stop();
    }
}