using Newtonsoft.Json;
using Pro4Soft.BackgroundWorker.Business.P4W.Entities.Base;

namespace Pro4Soft.BackgroundWorker.Business.P4W.Entities;

public class ProductP4: BaseP4Entity
{
    [JsonProperty("sku")]
    public string Sku { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("clientId")]
    public Guid? ClientId { get; set; }

    [JsonProperty("isPacksizeControlled")]
    public bool IsPacksizeController { get; set; }

    [JsonProperty("packsizes")]
    public List<PacksizeP4> Packsizes { get; set; } = [];
}

public class PacksizeP4
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("eachCount")]
    public int EachCount { get; set; }
}