using Newtonsoft.Json;
using Pro4Soft.BackgroundWorker.Business.P4W.Entities.Base;

namespace Pro4Soft.BackgroundWorker.Business.P4W.Entities;

public class WarehouseP4 : BaseP4Entity
{
    [JsonProperty("warehouseCode")]
    public string Code { get; set; }
}