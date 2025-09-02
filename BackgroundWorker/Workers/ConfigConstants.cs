// ReSharper disable InconsistentNaming

using Pro4Soft.BackgroundWorker.Execution.Common;

namespace Pro4Soft.BackgroundWorker.Business;

public class ConfigConstants : ConfigCollection
{
    #region Setup_System
    [ConfigDefinition(ConfigType.String, "Last download time of products")]
    public const string Download_Product_LastSync = nameof(Download_Product_LastSync);
    #endregion
}