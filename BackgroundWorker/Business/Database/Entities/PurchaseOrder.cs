using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using Pro4Soft.BackgroundWorker.Business.Database.Entities.Base;

namespace Pro4Soft.BackgroundWorker.Business.Database.Entities;

[Index(nameof(PurchaseOrderNumber), IsUnique = true)]
public class PurchaseOrder : DownloableP4WEntity
{
    public Guid VendorId { get; set; }
    public virtual Vendor Vendor { get; set; }

    public string WarehouseCode { get; set; }

    [Required]
    [MaxLength(36)]
    public string PurchaseOrderNumber { get; set; }

    public DateTime? RequiredDate { get; set; }
    public DateTime? CancelDate { get; set; }

    public string ReferenceNumber { get; set; }

    public string Reference1 { get; set; }

    public string Container { get; set; }
    public string Carrier { get; set; }
    public string Comments { get; set; }
    public string AppointmentNumber { get; set; }

    public string Info2 { get; set; }
    public string Info3 { get; set; }
    public string Info4 { get; set; }
    public string Info5 { get; set; }
    public string Info6 { get; set; }
    public string Info7 { get; set; }
    public string Info8 { get; set; }
    public string Info9 { get; set; }
    public string Info10 { get; set; }

    public bool Uploaded { get; set; }

    [ForeignKey(nameof(PurchaseOrderLine.PurchaseOrderId))]
    public virtual ICollection<PurchaseOrderLine> Lines { get; set; } = new List<PurchaseOrderLine>();
}

public class PurchaseOrderLine : EntityBase
{
    public Guid PurchaseOrderId { get; set; }
    public virtual PurchaseOrder PurchaseOrder { get; set; }

    public Guid ProductId { get; set; }
    public virtual Product Product { get; set; }

    public string Description { get; set; }

    public int LineNumber { get; set; }

    public string Info1 { get; set; }
    public string Info2 { get; set; }
    public string Info3 { get; set; }
    public string Info4 { get; set; }
    public string Info5 { get; set; }
    public string Info6 { get; set; }
    public string Info7 { get; set; }
    public string Info8 { get; set; }
    public string Info9 { get; set; }
    public string Info10 { get; set; }

    public int? Packsize { get; set; }
    public int? NumberOfPacks { get; set; }

    public decimal Quantity { get; set; }

    public decimal? ReceivedQuantity { get; set; }

    [ForeignKey(nameof(PurchaseOrderLineDetail.PurchaseOrderLineId))]
    public virtual ICollection<PurchaseOrderLineDetail> Details { get; set; } = new List<PurchaseOrderLineDetail>();
}

public class PurchaseOrderLineDetail : EntityBase
{
    public Guid PurchaseOrderLineId { get; set; }
    public virtual PurchaseOrderLine PurchaseOrderLine { get; set; }

    public decimal ReceivedQuantity { get; set; }
    public string LotNumber { get; set; }
    public string SerialNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int? PacksizeEachCount { get; set; }
}

public class PurchaseOrderLineMap : EntityBaseMap<PurchaseOrderLine>
{
    public override void Configure(EntityTypeBuilder<PurchaseOrderLine> builder)
    {
        builder
            .HasOne(c => c.Product)
            .WithMany(c => c.PurchaseOrderLines)
            .OnDelete(DeleteBehavior.NoAction);
    }
}