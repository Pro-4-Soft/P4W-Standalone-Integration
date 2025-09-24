using Newtonsoft.Json;
using Pro4Soft.BackgroundWorker.Dto.P4W.Entities.Base;

namespace Pro4Soft.BackgroundWorker.Dto.P4W.Entities;

public class CustomerReturnP4 : BaseP4Entity
{
    [JsonProperty("customerReturnNumber")]
    public string CustomerReturnNumber { get; set; }

    [JsonProperty("warehouseId")]
    public Guid? WarehouseId { get; set; }

    [JsonProperty("customerId")]
    public Guid? CustomerId { get; set; }

    [JsonProperty("vendor")]
    public CustomerP4 Customer { get; set; }

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
    public List<CustomerReturnLineP4> Lines { get; set; } = [];
}

public class CustomerReturnLineP4
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

    [JsonProperty("quantity")]
    public decimal Quantity { get; set; }

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

public class CustomerReturnLineDetailP4
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