namespace Pro4Soft.BackgroundWorker.Execution.SettingsFramework;

public class IntegrationSettings : Settings
{
    public string SqlConnection { get; set; }

    public string P4WUrl { get; set; }
    public string P4WApiKey { get; set; }

    public string TempFolder { get; set; }
}