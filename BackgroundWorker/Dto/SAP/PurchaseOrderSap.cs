namespace Pro4Soft.BackgroundWorker.Dto.SAP;

public class PurchaseOrderSap : BaseDocumentSap
{
    public string CardCode { get; set; }
    public string CardName { get; set; }
    public string NumAtCard { get; set; }
    public string Comments { get; set; }

    public string Confirmed { get; set; }
    public string Cancelled { get; set; }
    
    public string DocumentStatus { get; set; }

    public DateTime? UpdateDate { get; set; }
    public string UpdateTime { get; set; }
    public DateTime? ActualUpdated => UpdateDate?.Add(TimeSpan.Parse(UpdateTime ?? "00:00:00"));

    public AddressSap AddressExtension { get; set; }
    public List<PurchaseOrderLineSap> DocumentLines { get; set; } = [];
}

public class PurchaseOrderLineSap
{
    public int LineNum { get; set; }
    public string ItemCode { get; set; }
    public string ItemDescription { get; set; }
    public decimal? UnitsOfMeasurment { get; set; }
    public decimal RemainingOpenQuantity { get; set; }
    public decimal? SalesPrice { get; set; }
    public string WarehouseCode { get; set; }
}