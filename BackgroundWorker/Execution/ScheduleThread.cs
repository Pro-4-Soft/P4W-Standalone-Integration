using System.Diagnostics;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;

namespace Pro4Soft.BackgroundWorker.Execution;

public class ScheduleThread
{
    private static ScheduleThread _instance;
    public static ScheduleThread Instance => _instance ??= new();

    internal static Lazy<Process> Process => new(System.Diagnostics.Process.GetCurrentProcess);

    readonly AutoResetEvent _resetEvent = new(false);

    private bool _isRunning = true;
    private Thread _scheduleThread;
    private List<WorkerThread> _workerThreads = new();

    private void RunScheduling()
    {
        _workerThreads.ForEach(c => c.Stop());
        _workerThreads = App<Settings>.Instance.Schedules.Select(c => c.ThreadName).Distinct().Select(c => new WorkerThread(c)).ToList();

        foreach (var taskOnStartup in App<Settings>.Instance.Schedules
                     .Where(c => c.Active)
                     .Where(c => c.RunOnStartup))
            RunTask(taskOnStartup);

        while (_isRunning)
        {
            var now = DateTimeOffset.Now;
            var (sleepTime, tasksToExecute) = App<Settings>.Instance.NextTime(now);
            if (!tasksToExecute.Any())
            {
                BaseWorker.LogAsync($"Nothing to execute").Wait();
                break;
            }

            _resetEvent.WaitOne(sleepTime);
            if (!_isRunning)
                break;

            foreach (var poll in tasksToExecute)
                RunTask(poll);
            //Wait a little bit to avoid double execution
            Thread.Sleep(TimeSpan.FromMilliseconds(50));
        }

        BaseWorker.LogAsync($"Stopped").Wait();
    }

    public void RunTask(ScheduleSetting task)
    {
        if (task == null)
            return;

        _workerThreads.SingleOrDefault(c => c.Name == task.ThreadName)?.ScheduleExecute(task);
    }

    public void Start()
    {
        _scheduleThread = new(RunScheduling);
        _scheduleThread.Start();
    }

    public void Stop()
    {
        _isRunning = false;
        _resetEvent.Set();
        _scheduleThread.Join();
        _workerThreads.ForEach(c => c.Stop());
        _workerThreads = new();
    }
}