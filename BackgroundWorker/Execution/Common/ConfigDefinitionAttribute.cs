using System.Reflection;

namespace Pro4Soft.BackgroundWorker.Execution.Common;

public class ConfigDefinitionAttribute(ConfigType type, string description = null, object defaultValue = null, string[] multiSelectOptions = null, string accountType = null) : Attribute
{
    public ConfigType Type { get; set; } = type;
    public string AccountType { get; set; } = accountType;
    public string Description { get; set; } = description;
    public string[] MultiSelectOptions { get; set; } = multiSelectOptions;
    public object DefaultValue { get; set; } = defaultValue;
    
    public static ConfigDefinitionAttribute GetConfigAttr<T>(string name) where T:ConfigCollection
    {
        var prop = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static).SingleOrDefault(c => c.Name == name) ?? 
                   throw new BusinessWebException($"Config Property [{name}] does not exist");
        if (GetCustomAttribute(prop, typeof(ConfigDefinitionAttribute)) is not ConfigDefinitionAttribute attr)
            throw new BusinessWebException($"Config Property Attribute is missing");
        return attr;
    }
}

public class ConfigCollection
{

}

public enum ConfigType
{
    String,
    MultilineString,
    EmailBody,
    Password,
    Int,
    Double,
    Bool,
    MultiSelect,
    Account
}