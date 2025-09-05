using Newtonsoft.Json;
using Pro4Soft.BackgroundWorker.Business.P4W.Entities.Base;

namespace Pro4Soft.BackgroundWorker.Business.P4W.Entities;

public class AdjustmentP4 : BaseP4Entity
{
    [JsonProperty("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    [JsonProperty("client")]
    public string Client { get; set; }

    [JsonProperty("subType")]
    public string SubType { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("fromWarehouse")]
    public string FromWarehouse { get; set; }

    [JsonProperty("toWarehouse")]
    public string ToWarehouse { get; set; }

    [JsonProperty("productId")]
    public Guid ProductId { get; set; }

    [JsonProperty("sku")]
    public string Sku { get; set; }

    [JsonProperty("eachCount")]
    public int EachCount { get; set; }

    [JsonProperty("numberOfPacks")]
    public int NumberOfPacks { get; set; }

    [JsonProperty("lotNumber")]
    public string LotNumber { get; set; }

    [JsonProperty("expiryDate")]
    public DateTimeOffset ExpiryDate { get; set; }

    [JsonProperty("serialNumber")]
    public string SerialNumber { get; set; }

    [JsonProperty("quantity")]
    public decimal Quantity { get; set; }

    [JsonProperty("reason")]
    public string Reason { get; set; }
}