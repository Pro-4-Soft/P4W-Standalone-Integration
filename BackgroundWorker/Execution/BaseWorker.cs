using System.Collections.Concurrent;
using System.IO;
using Pro4Soft.BackgroundWorker.Dto.P4W.Entities;
using Pro4Soft.BackgroundWorker.Execution.Common;
using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;
using Pro4Soft.BackgroundWorker.Workers;

namespace Pro4Soft.BackgroundWorker.Execution;

public abstract class BaseWorker
{
    protected static ConcurrentDictionary<string, SapServiceClient> SapServiceClientCache = new();

    protected ScheduleSetting Settings { get; }
    protected readonly IntegrationSettings Config = App<IntegrationSettings>.Instance;
    protected readonly WebClient P4WClient;

    private static readonly string LogFilePath;

    // Static constructor — runs once when the WebJob starts
    static BaseWorker()
    {
        LogFilePath = InitializeLogDirectory();
    }

    protected BaseWorker(ScheduleSetting settings)
    {
        Settings = settings;
        P4WClient = new(Config.P4WUrl, Config.P4WApiKey);
    }

    public virtual void Execute() => ExecuteAsync().Wait();

    public virtual async Task ExecuteAsync() => await Task.CompletedTask;

    public static async Task LogAsync(string msg)
    {
        msg = $"Data [{DateTime.Now:G}] {msg}";
        try
        {
            Utils.AppendTextFile(LogFilePath, msg);
        }
        catch { }

        await Console.Out.WriteLineAsync(msg);
    }

    public static async Task LogErrorAsync(Exception ex) => await LogErrorAsync(ex.ToString());

    protected static async Task LogErrorAsync(string msg)
    {
        msg = $"Error [{DateTime.Now:G}] {msg}";
        try
        {
            Utils.AppendTextFile(LogFilePath, msg);
        }
        catch { }

        await Console.Error.WriteLineAsync(msg);
    }

    // Business logic helpers
    protected async Task<Guid?> GetClientId(CompanySettings company)
    {
        var clients = await P4WClient.GetInvokeAsync<List<ClientP4>>($"clients?clientName={company.P4WClientName}");
        if (clients.Count == 0)
            throw new BusinessWebException($"Client [{company.P4WClientName}] does not exist in P4W");
        return clients.First().Id;
    }

    /// <summary>
    /// Determines the correct logs folder and creates it + log file at startup.
    /// Primary: %WEBJOBS_DATA_PATH%\Logs
    /// Fallback: %HOME%\data\jobs\continuous\&lt;JOB_NAME&gt;\Logs
    /// Local fallback: BaseDirectory\Logs
    /// </summary>
    private static string InitializeLogDirectory()
    {
        // 1️⃣ Get primary Azure path
        var dataPath = Environment.GetEnvironmentVariable("WEBJOBS_DATA_PATH");

        // 2️⃣ Fallbacks
        if (string.IsNullOrWhiteSpace(dataPath))
        {
            var home = Environment.GetEnvironmentVariable("HOME");         // e.g., D:\home
            var jobName = Environment.GetEnvironmentVariable("WEBJOBS_NAME");
            var jobType = Environment.GetEnvironmentVariable("WEBJOBS_TYPE");
            if (!string.IsNullOrWhiteSpace(home) &&
                !string.IsNullOrWhiteSpace(jobName) &&
                string.Equals(jobType, "Continuous", StringComparison.OrdinalIgnoreCase))
            {
                dataPath = Path.Combine(home, "data", "jobs", "continuous", jobName);
            }
            else
            {
                dataPath = AppDomain.CurrentDomain.BaseDirectory;
            }
        }

        // 3️⃣ Ensure Logs folder exists
        var logsDir = Path.Combine(dataPath, "Logs");
        try
        {
            if (!Directory.Exists(logsDir))
                Directory.CreateDirectory(logsDir);
        }
        catch
        {
            logsDir = AppDomain.CurrentDomain.BaseDirectory;
        }

        // 4️⃣ Create the log file if it doesn’t exist
        var logFile = Path.Combine(logsDir, Utils.ProcessName + ".log");
        try
        {
            if (!File.Exists(logFile))
            {
                using var fs = File.Create(logFile);
                using var sw = new StreamWriter(fs);
                sw.WriteLine($"[INFO] Log file created at startup {DateTime.Now:G}");
            }
        }
        catch { }

#if DEBUG
        Console.WriteLine($"[DEBUG] Logs initialized at: {logFile}");
#endif

        return logFile;
    }
}
