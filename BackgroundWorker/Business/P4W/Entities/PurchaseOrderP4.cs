using Newtonsoft.Json;
using Pro4Soft.BackgroundWorker.Business.P4W.Entities.Base;

namespace Pro4Soft.BackgroundWorker.Business.P4W.Entities;

public class PurchaseOrderP4 : BaseP4Entity
{
    [JsonProperty("purchaseOrderNumber")]
    public string PurchaseOrderNumber { get; set; }

    [JsonProperty("warehouseId")]
    public Guid? WarehouseId { get; set; }

    [JsonProperty("vendorId")]
    public Guid? VendorId { get; set; }

    [JsonProperty("vendor")]
    public VendorP4 Vendor { get; set; }

    [JsonProperty("warehouse")]
    public VendorP4 Warehouse { get; set; }

    [JsonProperty("comments")]
    public string Comments { get; set; }

    [JsonProperty("reference1")]
    public string Reference1 { get; set; }
    
    [JsonProperty("reference2")]
    public string Reference2 { get; set; }

    [JsonProperty("reference3")]
    public string Reference3 { get; set; }

    [JsonProperty("lines")] 
    public List<PurchaseOrderLineP4> Lines { get; set; } = [];
}

public class PurchaseOrderLineP4
{
    [JsonProperty("lineNumber")]
    public int LineNumber { get; set; }

    [JsonProperty("productId")]
    public Guid? ProductId { get; set; }

    [JsonProperty("product")]
    public ProductP4 Product { get; set; }

    [JsonProperty("numberOfPacks")]
    public int? NumberOfPacks { get; set; }

    [JsonProperty("packsize")]
    public int? Packsize { get; set; }

    [JsonProperty("orderedQuantity")]
    public decimal OrderedQuantity { get; set; }

    [JsonProperty("receivedQuantity")]
    public decimal? ReceivedQuantity { get; set; }

    [JsonProperty("referenceNumber")]
    public string ReferenceNumber { get; set; }

    [JsonProperty("reference1")]
    public string Reference1 { get; set; }

    [JsonProperty("reference2")]
    public string Reference2 { get; set; }

    [JsonProperty("reference3")]
    public string Reference3 { get; set; }

    [JsonProperty("details")]
    public List<PurchaseOrderLineDetailP4> Details { get; set; }
}

public class PurchaseOrderLineDetailP4
{
    [JsonProperty("receivedQuantity")]
    public decimal ReceivedQuantity { get; set; }
    
    [JsonProperty("lotNumber")]
    public string LotNumber { get; set; }
    
    [JsonProperty("serialNumber")]
    public string SerialNumber { get; set; }
    
    [JsonProperty("expiryDate")]
    public DateTime? ExpiryDate { get; set; }

    [JsonProperty("packsizeEachCount")]
    public int? PacksizeEachCount { get; set; }
}