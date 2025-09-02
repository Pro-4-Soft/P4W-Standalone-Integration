using Pro4Soft.BackgroundWorker.Business.Database;
using Pro4Soft.BackgroundWorker.Business.SAP;
using Pro4Soft.BackgroundWorker.Execution.Common;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;

namespace Pro4Soft.BackgroundWorker.Execution;

public abstract class BaseWorker
{
    protected ScheduleSetting Settings { get; }
    protected readonly IntegrationSettings Config = App<IntegrationSettings>.Instance;

    protected readonly WebClient P4WClient;
    //protected readonly SapServiceClient SapClient;

    protected BaseWorker(ScheduleSetting settings)
    {
        Settings = settings;
        P4WClient = new(Config.P4WUrl, Config.P4WApiKey);
        //SapClient = new(Config.SapUrl, Config.SapCompanyDb, Config.SapUsername, Config.SapPassword);
    }

    public virtual void Execute()
    {
        ExecuteAsync().Wait();
    }

    public virtual async Task ExecuteAsync()
    {
        await Task.CompletedTask;
    }

    public static async Task LogAsync(string msg)
    {
        msg = $"Data [{DateTime.Now:G}] {msg}";
        try
        {
            Utils.AppendTextFile(Path.Combine(App<IntegrationSettings>.Instance.TempFolder, "Logs", Utils.ProcessName + ".log"), msg);
        }
        catch{}
        
        await Console.Out.WriteLineAsync(msg);
    }

    public static async Task LogErrorAsync(Exception ex)
    {
        await LogErrorAsync(ex.ToString());
    }

    public static async Task LogErrorAsync(string msg)
    {
        msg = $"Error [{DateTime.Now:G}] {msg}";
        try
        {
            Utils.AppendTextFile(Path.Combine(App<IntegrationSettings>.Instance.TempFolder, "Logs", Utils.ProcessName + ".log"), msg);
        }
        catch { }

        await Console.Error.WriteLineAsync(msg);
    }
}