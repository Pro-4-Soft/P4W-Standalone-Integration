using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Business.Database.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Pro4Soft.BackgroundWorker.Business.Database.Entities;

[Index(nameof(Sku), IsUnique = true)]
public class Product : P4WStateEntity
{
    public Guid ClientId { get; set; }

    [Required]
    [MaxLength(36)]
    public string Sku { get; set; }

    public string Description { get; set; }
    public string Category { get; set; }
    public string Upc { get; set; }

    public string Nmfc { get; set; }
    public string CommodityDescription { get; set; }
    public string FreightClass { get; set; }

    public decimal? Weight { get; set; }
    public decimal? Height { get; set; }
    public decimal? Width { get; set; }
    public decimal? Length { get; set; }

    public bool IsInventoryItem { get; set; }

    public bool IsExpiryControlled { get; set; }
    public bool IsPacksizeControlled { get; set; }
    public bool IsSerialControlled { get; set; }
    public bool IsLotControlled { get; set; }

    public string Info1 { get; set; }
    public string Info2 { get; set; }
    public string Info3 { get; set; }
    public string Info4 { get; set; }
    public string Info6 { get; set; }
    public string Info7 { get; set; }
    public string Info8 { get; set; }
    public string Info9 { get; set; }
    public string Info10 { get; set; }

    //Small Parcel
    public string HtsNumber { get; set; }
    public string CountryOfOrigin { get; set; }

    [ForeignKey(nameof(Adjustment.ProductId))]
    public virtual ICollection<Adjustment> Adjustments { get; set; } = new List<Adjustment>();

    [ForeignKey(nameof(ProductInventory.ProductId))]
    public virtual ICollection<ProductInventory> Inventory { get; set; } = new List<ProductInventory>();

    [ForeignKey(nameof(Packsize.ProductId))]
    public virtual ICollection<Packsize> Packsizes { get; set; } = new List<Packsize>();

    [ForeignKey(nameof(PurchaseOrderLine.ProductId))]
    public virtual ICollection<PurchaseOrderLine> PurchaseOrderLines { get; set; } = new List<PurchaseOrderLine>();

    [ForeignKey(nameof(CustomerReturnLine.ProductId))]
    public virtual ICollection<CustomerReturnLine> CustomerReturnLines { get; set; } = new List<CustomerReturnLine>();

    [ForeignKey(nameof(PickTicketLine.ProductId))]
    public virtual ICollection<PickTicketLine> PickTicketLines { get; set; } = new List<PickTicketLine>();
}

public class Packsize : EntityBase
{
    public Guid ProductId { get; set; }
    public virtual Product Product { get; set; }

    public decimal? Weight { get; set; }
    public decimal? Height { get; set; }
    public decimal? Width { get; set; }
    public decimal? Length { get; set; }

    public string Name { get; set; }
    public int EachCount { get; set; }
}

public class ProductMap : EntityBaseMap<Product>
{
    public override void Configure(EntityTypeBuilder<Product> builder)
    {
        builder
            .HasMany(c => c.PickTicketLines)
            .WithOne(c => c.Product)
            .OnDelete(DeleteBehavior.Restrict);
    }
}