using Newtonsoft.Json;
using Pro4Soft.BackgroundWorker.Dto.P4W.Entities.Base;

namespace Pro4Soft.BackgroundWorker.Dto.P4W.Entities;

public sealed class PickTicketP4 : BaseP4Entity
{
    [JsonProperty("referenceNumber")]
    public string ReferenceNumber { get; set; }

    [JsonProperty("poNumber")]
    public string PoNumber { get; set; }

    [JsonProperty("comments")]
    public string Comments { get; set; }

    [JsonProperty("pickingInstructions")]
    public string PickingInstructions { get; set; }

    [JsonProperty("requiredDate")]
    public DateTimeOffset? RequiredDate { get; set; }

    [JsonProperty("cancelDate")]
    public DateTimeOffset? CancelDate { get; set; }

    [JsonProperty("freightType")]
    public string FreightType { get; set; }

    [JsonProperty("routeNumber")]
    public string RouteNumber { get; set; }

    [JsonProperty("freightTerms")]
    public string FreightTerms { get; set; }

    [JsonProperty("carrier")]
    public string Carrier { get; set; }

    [JsonProperty("shippingService")]
    public string ShippingService { get; set; }

    [JsonProperty("shipCode")]
    public string ShipCode { get; set; }

    [JsonProperty("closeDate")]
    public DateTimeOffset? CloseDate { get; set; }

    [JsonProperty("warehouseId")]
    public Guid WarehouseId { get; set; }

    [JsonProperty("warehouse")]
    public WarehouseP4 Warehouse { get; set; }

    [JsonProperty("customerId")]
    public Guid CustomerId { get; set; }

    [JsonProperty("customer")]
    public CustomerP4 Customer { get; set; }

    [JsonProperty("pickTicketNumber")]
    public string PickTicketNumber { get; set; }

    [JsonProperty("shipFrom")]
    public AddressP4 ShipFrom { get; set; }

    [JsonProperty("shipTo")]
    public AddressP4 ShipTo { get; set; }

    [JsonProperty("billTo")]
    public AddressP4 BillTo { get; set; }

    [JsonProperty("lines")]
    public List<PickTicketLineP4> Lines { get; set; }

    [JsonProperty("totes")]
    public List<ToteP4> Totes { get; set; }
}

public sealed class AddressP4 : BaseP4Entity
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("phone")]
    public string Phone { get; set; }

    [JsonProperty("email")]
    public string Email { get; set; }

    [JsonProperty("address1")]
    public string Address1 { get; set; }

    [JsonProperty("address2")]
    public string Address2 { get; set; }

    [JsonProperty("city")]
    public string City { get; set; }

    [JsonProperty("stateProvince")]
    public string StateProvince { get; set; }

    [JsonProperty("zipPostal")]
    public string ZipPostal { get; set; }

    [JsonProperty("country")]
    public string Country { get; set; }
}

public sealed class PickTicketLineP4 : BaseP4Entity
{
    [JsonProperty("lineNumber")]
    public int LineNumber { get; set; }

    [JsonProperty("packsize")]
    public int? Packsize { get; set; }

    [JsonProperty("numberOfPacks")]
    public int? NumberOfPacks { get; set; }

    [JsonProperty("productId")]
    public Guid? ProductId { get; set; }

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

public sealed class ToteP4 : BaseP4Entity
{
    [JsonProperty("sscc18Code")]
    public string Sscc18Code { get; set; } = string.Empty;

    [JsonProperty("bolNumber")]
    public string BolNumber { get; set; } = string.Empty;

    [JsonProperty("cartonNumber")]
    public int CartonNumber { get; set; }

    [JsonProperty("carrier")]
    public string Carrier { get; set; } = string.Empty;

    [JsonProperty("shippingService")]
    public string ShippingService { get; set; } = string.Empty;

    [JsonProperty("shippingCost")]
    public decimal? ShippingCost { get; set; }

    [JsonProperty("shippingCostCurrency")]
    public string ShippingCostCurrency { get; set; } = string.Empty;

    [JsonProperty("trackTraceNumber")]
    public string TrackTraceNumber { get; set; } = string.Empty;

    [JsonProperty("lines")]
    public List<ToteLineP4> Lines { get; set; } = [];
}

public sealed class ToteLineP4 : BaseP4Entity
{
    [JsonProperty("pickedQuantity")]
    public decimal PickedQuantity { get; set; }

    [JsonProperty("shippedQuantity")]
    public decimal ShippedQuantity { get; set; }

    [JsonProperty("deliveredQuantity")]
    public decimal? DeliveredQuantity { get; set; }

    [JsonProperty("pickTicketLineId")]
    public Guid PickTicketLineId { get; set; }

    [JsonProperty("product")]
    public ProductP4 Product { get; set; } = new();

    [JsonProperty("details")]
    public List<ToteLineDetail> Details { get; set; } = [];
}

public sealed class ToteLineDetail : BaseP4Entity
{
    [JsonProperty("pickedQuantity")]
    public decimal PickedQuantity { get; set; }

    [JsonProperty("shippedQuantity")]
    public decimal ShippedQuantity { get; set; }

    [JsonProperty("deliveredQuantity")]
    public decimal? DeliveredQuantity { get; set; }

    [JsonProperty("lotNumber")]
    public string LotNumber { get; set; } = string.Empty;

    [JsonProperty("serialNumber")]
    public string SerialNumber { get; set; } = string.Empty;

    [JsonProperty("expiryDate")]
    public DateTime? ExpiryDate { get; set; }

    [JsonProperty("packsizeEachCount")]
    public int? PacksizeEachCount { get; set; }
}