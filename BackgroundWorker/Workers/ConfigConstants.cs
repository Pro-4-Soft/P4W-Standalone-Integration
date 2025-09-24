// ReSharper disable InconsistentNaming

using Pro4Soft.BackgroundWorker.Execution.Common;

namespace Pro4Soft.BackgroundWorker.Workers;

public class ConfigConstants : ConfigCollection
{
    [ConfigDefinition(ConfigType.String, "Last download time of products")]
    public const string Download_Product_LastSync = nameof(Download_Product_LastSync);

    [ConfigDefinition(ConfigType.String, "Last download time of purchase orders")]
    public const string Download_PurchaseOrder_LastSync = nameof(Download_PurchaseOrder_LastSync);

    [ConfigDefinition(ConfigType.String, "Last return requests time of purchase orders")]
    public const string Download_ReturnRequest_LastSync = nameof(Download_ReturnRequest_LastSync);
    
    [ConfigDefinition(ConfigType.String, "Last download time of sales orders")]
    public const string Download_SalesOrder_LastSync = nameof(Download_SalesOrder_LastSync);
}