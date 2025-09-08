namespace Pro4Soft.BackgroundWorker.Business.SAP;

public class SalesOrderSap : BaseDocumentSap
{
    public string CardCode { get; set; }
    public string CardName { get; set; }

    public string Address { get; set; }

    public string Comments { get; set; }

    public string Confirmed { get; set; }
    public string DocumentStatus { get; set; }

    public string Cancelled { get; set; }

    public string DocObjectCode { get; set; }

    public DateTime? RequriedDate { get; set; }
    public DateTime? CancelDate { get; set; }

    public DateTime? UpdateDate { get; set; }
    public string UpdateTime { get; set; }
    public DateTime? ActualUpdated => UpdateDate?.Add(TimeSpan.Parse(UpdateTime ?? "00:00:00"));

    public AddressSap AddressExtension { get; set; }
    public List<SalesOrderLineSap> DocumentLines { get; set; } = [];
}

public class SalesOrderLineSap
{
    public int LineNum { get; set; }
    public string ItemCode { get; set; }
    public string ItemDescription { get; set; }
    public decimal? UnitsOfMeasurment { get; set; }
    public decimal RemainingOpenQuantity { get; set; }
    public decimal Price{ get; set; }
    public string WarehouseCode { get; set; }
}