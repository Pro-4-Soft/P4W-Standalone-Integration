using Pro4Soft.BackgroundWorker.Dto.Database.Entities.Base;

namespace Pro4Soft.BackgroundWorker.Dto.Database.Entities;

public class ProductInventory : P4WStateEntity
{
    public Guid ProductId { get; set; }
    public virtual Product Product { get; set; }
    
    public string WarehouseCode { get; set; }

    public decimal Quantity { get; set; }

    public List<ProductInventoryDetail> Details { get; set; } = [];
}

public class ProductInventoryDetail : EntityBase
{
    public Guid ProductInventoryId { get; set; }
    public virtual ProductInventory ProductInventory { get; set; }

    public decimal Quantity { get; set; }
    public string LotNumber { get; set; }
    public string SerialNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int? PacksizeEachCount { get; set; }
}