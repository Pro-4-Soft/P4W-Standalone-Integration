using Pro4Soft.BackgroundWorker.Execution.SettingsFramework;

namespace Pro4Soft.BackgroundWorker;

public class IntegrationSettings : Settings
{
    public string SqlConnection { get; set; }

    public string P4WUrl { get; set; }
    public string P4WApiKey { get; set; }

    public string TempFolder { get; set; }
}