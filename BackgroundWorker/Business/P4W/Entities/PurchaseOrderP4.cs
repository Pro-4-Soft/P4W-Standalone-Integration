using Newtonsoft.Json;
using Pro4Soft.BackgroundWorker.Business.P4W.Entities.Base;

namespace Pro4Soft.BackgroundWorker.Business.P4W.Entities;

public class PurchaseOrderP4 : BaseP4Entity
{
    [JsonProperty("purchaseOrderNumber")]
    public string PurchaseOrderNumber { get; set; }

    [JsonProperty("comments")]
    public string Comments { get; set; }

    [JsonProperty("warehouseId")]
    public Guid WarehouseId { get; set; }

    [JsonProperty("vendorId")]
    public Guid VendorId { get; set; }

    [JsonProperty("lines")] 
    public List<PurchaseOrderLineP4> Lines { get; set; } = [];
}

public class PurchaseOrderLineP4
{
    [JsonProperty("lineNumber")]
    public int LineNumber { get; set; }

    [JsonProperty("productId")]
    public Guid ProductId { get; set; }

    [JsonProperty("orderedQuantity")]
    public decimal OrderedQuantity { get; set; }

    [JsonProperty("referenceNumber")]
    public string ReferenceNumber { get; set; }

    [JsonProperty("reference1")]
    public string Reference1 { get; set; }

    [JsonProperty("reference2")]
    public string Reference2 { get; set; }

    [JsonProperty("reference3")]
    public string Reference3 { get; set; }
}