using System.Text.Json;

namespace Pro4Soft.BackgroundWorker.Business.SAP;

public class ProductSap: BaseSapEntity
{
    public string ItemCode { get; set; }
    public string ItemName { get; set; }
        
    public int? ItemsGroupCode { get; set; }

    public string BarCode { get; set; }

    public decimal? PurchaseUnitLength { get; set; }
    public decimal? PurchaseUnitWidth { get; set; }
    public decimal? PurchaseUnitHeight { get; set; }
    public decimal? PurchaseUnitWeight { get; set; }

    public string ManageSerialNumbers { get; set; }
    public string ManageBatchNumbers { get; set; }

    public int UoMGroupEntry { get;set; }

    public DateTime? CreateDate { get; set; }
    public string CreateTime { get; set; }
    
    public DateTime? ActualUpdated => UpdateDate?.Add(TimeSpan.Parse(UpdateTime ?? "00:00:00"));

    public List<ItemPreferredVendor> ItemPreferredVendors { get; set; }
}

public class ItemPreferredVendor
{
    public string BPCode { get; set; }
}