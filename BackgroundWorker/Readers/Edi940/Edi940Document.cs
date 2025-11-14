namespace Pro4Soft.BackgroundWorker.Readers.Edi940;

public class Edi940Document
{
    public string Partner { get; set; }
    public string Route { get; set; }
    public string Company { get; set; }
    public string Set { get; set; }
    public Edi940Header Header { get; set; }
    public List<Edi940LineItem> LineItems { get; set; } = new();
}

public class Edi940Header
{
    public string OrderNumber { get; set; }
    public string RouteName { get; set; }
    public string Reference { get; set; }
    public string ShipFromLocation { get; set; }
    public DateTime? ShipDate { get; set; }
}

public class Edi940LineItem
{
    public decimal Quantity { get; set; }
    public string UnitOfMeasure { get; set; }
    public string ProductCode { get; set; }
}