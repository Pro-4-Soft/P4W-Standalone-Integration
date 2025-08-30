using Pro4Soft.BackgroundWorker.Business.Database.Entities.Base;

namespace Pro4Soft.BackgroundWorker.Business.Database.Entities;

public class Adjustment : EntityBase
{
    public Guid ProductId { get; set; }
    public virtual Product Product { get; set; }

    public Guid WarehouseId { get; set; }
    public virtual Warehouse Warehouse { get; set; }

    public Guid? PacksizeId { get; set; }
    public virtual Packsize Packsize { get; set; }

    public int? EachCount { get; set; }
    public int? Packs { get; set; }
    public int Quantity { get; set; }

    public string Type { get; set; }
    public string Reason { get; set; }

    public DateTimeOffset Date { get; set; }
}