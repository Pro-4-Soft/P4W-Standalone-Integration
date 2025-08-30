using Newtonsoft.Json;
using Pro4Soft.BackgroundWorker.Execution.Common;

namespace Pro4Soft.BackgroundWorker.Execution.SettingsFramework;

public class AppSettings
{
    protected virtual string SettingsFileName => "settings.json";

    public T Initialize<T>() where T : AppSettings, new()
    {
        var newFile = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"..", SettingsFileName));
        if(!newFile.Exists)
            newFile = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SettingsFileName));

        return Utils.DeserializeFromJson(Utils.ReadTextFile(newFile.FullName, false), null, false, new T());
    }

    public string Serialize()
    {
        return Utils.SerializeToStringJson(this, Formatting.Indented);
    }

    public void SaveToFile()
    {
        Utils.WriteTextFile(Path.Combine(Directory.GetCurrentDirectory(), SettingsFileName), Serialize());
    }
}

public class App<T> where T : AppSettings, new()
{
    private static readonly Lazy<T> _instance = new(() => Activator.CreateInstance<T>().Initialize<T>());
    public static readonly T Instance = _instance.Value;
}