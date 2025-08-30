namespace Pro4Soft.BackgroundWorker.Execution.SettingsFramework;

[AttributeUsage(AttributeTargets.Class)]
public class IntegrationSettingsAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}