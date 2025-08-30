namespace Pro4Soft.BackgroundWorker.Execution.SettingsFramework;

public class Settings:AppSettings
{
    public List<ScheduleSetting> Schedules { get; set; } = [];
    public (TimeSpan,List<ScheduleSetting>) NextTime(DateTimeOffset now)
    {
        var smallestTimespan = TimeSpan.MaxValue;
        var schedules = new List<ScheduleSetting>();
        foreach (var schedule in Schedules
                     .Where(c => c.Start != null)
                     .Where(c=>c.Sleep != null)
                     .Where(c=>c.Active))
        {
            var pollTimeout = schedule.GetTimeout(now);
            if (smallestTimespan == pollTimeout)
                schedules.Add(schedule);
            else if (smallestTimespan > pollTimeout)
            {
                smallestTimespan = pollTimeout;
                schedules = [schedule];
            }
        }
        return (smallestTimespan,schedules);
    }
}

public class ScheduleSetting
{
    public string Name { get; set; }
    public bool Active { get; set; } = true;
    public bool RunOnStartup { get; set; } = false;
    public DateTimeOffset? Start { get; set; }
    public TimeSpan? Sleep { get; set; }
    public string Class { get; set; }
    public string ThreadName { get; set; } = "Default";

    public TimeSpan GetTimeout(DateTimeOffset now)
    {
        if (Sleep == null)
            return TimeSpan.MaxValue;

        Start ??= now;

        var totalMillis = (long)now.Subtract(Start.Value).TotalMilliseconds;
        var wholeIntervals = totalMillis / (long)Sleep?.TotalMilliseconds;
        var nextRun = Start?.Add(TimeSpan.FromMilliseconds((wholeIntervals + 1) * (long)Sleep?.TotalMilliseconds));
        return nextRun.Value.Subtract(now);
    }
}