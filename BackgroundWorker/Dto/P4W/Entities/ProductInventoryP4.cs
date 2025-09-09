using Newtonsoft.Json;

namespace Pro4Soft.BackgroundWorker.Dto.P4W.Entities;

public class ProductInventoryP4
{
    [JsonProperty("productId")]
    public Guid ProductId { get; set; }

    [JsonProperty("sku")]
    public string Sku { get; set; }

    [JsonProperty("clientId")]
    public Guid ClientId { get; set; }

    [JsonProperty("warehouseId")]
    public Guid WarehouseId { get; set; }

    [JsonProperty("quantity")]
    public int Quantity { get; set; }

    [JsonProperty("isSerialControlled")]
    public bool IsSerialControlled { get; set; }

    [JsonProperty("isExpiryControlled")]
    public bool IsExpiryControlled { get; set; }

    [JsonProperty("isDecimalControlled")]
    public bool IsDecimalControlled { get; set; }

    [JsonProperty("details")]
    public List<ProductInventoryDetailP4> Details { get; set; }
}

public class ProductInventoryDetailP4
{
    [JsonProperty("serialNumber")]
    public string SerialNumber { get; set; }

    [JsonProperty("lotNumber")]
    public string LotNumber { get; set; }

    [JsonProperty("expiry")]
    public DateTime? ExpiryDate { get; set; }

    [JsonProperty("eachCount")]
    public int? PacksizeEachCount { get; set; }

    [JsonProperty("quantity")]
    public decimal Quantity { get; set; }
}