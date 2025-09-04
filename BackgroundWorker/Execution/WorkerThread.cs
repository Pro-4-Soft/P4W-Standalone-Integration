using System.Reflection;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;

namespace Pro4Soft.BackgroundWorker.Execution;

public class WorkerThread
{
    public readonly string Name;
    private readonly Thread _thread;
    private readonly AutoResetEvent _autoReset = new(false);
    private readonly List<ScheduleSetting> _queue = new();
    private static readonly object lockObj = new();

    public WorkerThread(string name)
    {
        Name = name;
        _thread = new(Run)
        {
            Name = name
        };
        _thread.Start();
    }

    public void ScheduleExecute(ScheduleSetting poll)
    {
        if (poll == null)
            return;
        lock (lockObj)
        {
            if (_queue.Any(c => c.Name == poll.Name))
                return;
            _queue.Add(poll);
        }
        _autoReset.Set();
    }

    private bool _pendingStop;

    public void Stop()
    {
        lock (lockObj)
            _queue.Clear();

        _pendingStop = true;
        _autoReset.Set();
        _thread.Join(TimeSpan.FromSeconds(10));
        if (!_thread.IsAlive)
            return;

        //TODO: Rodion: Figure out how to force kill a thread
        //_thread.Abort("Stop grace period expired");

        _thread.Join(TimeSpan.FromSeconds(20));
    }

    private void Run()
    {
        Console.Out.WriteLineAsync($"Thread: [{Thread.CurrentThread.Name}] started");
        while (!_pendingStop)
        {
            try
            {
                bool sleep;
                lock (lockObj)
                    sleep = !_queue.Any();
                if (sleep)
                {
                    _autoReset.WaitOne(TimeSpan.FromSeconds(30));
                    continue;
                }

                ScheduleSetting task;
                lock (lockObj)
                {
                    task = _queue[0];
                    _queue.RemoveAt(0);
                }

                //lock (lockObj)
                //    BaseWorker.LogAsync($"Thread: [{Thread.CurrentThread.Name}]: [{task.Name}] started.").Wait();

                var type = Assembly.GetEntryAssembly()?.GetType(task.Class);
                if (type == null)
                    throw new($"Thread: [{Thread.CurrentThread.Name}]: Invalid type: [{task.Class}]");
                if (Activator.CreateInstance(type, task) is not BaseWorker worker)
                    return;
                try
                {
                    worker.Execute();

                    //lock (lockObj)
                    //    BaseWorker.LogAsync($"Thread: [{Thread.CurrentThread.Name}]: [{task.Name}] finished.").Wait();
                }
                catch (Exception e)
                {
                    BaseWorker.LogErrorAsync(new Exception($"Thread: [{Thread.CurrentThread.Name}]", e)).Wait();
                }
                
            }
            catch (Exception e)
            {
                BaseWorker.LogErrorAsync(new Exception($"Thread: [{Thread.CurrentThread.Name}]", e)).Wait();
            }
        }
        BaseWorker.LogAsync($"Thread: [{Thread.CurrentThread.Name}] stopped").Wait();
    }
}