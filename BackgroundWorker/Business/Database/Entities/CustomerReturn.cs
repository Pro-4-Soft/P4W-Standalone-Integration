using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pro4Soft.BackgroundWorker.Business.Database.Entities.Base;

namespace Pro4Soft.BackgroundWorker.Business.Database.Entities;

[Index(nameof(CustomerReturnNumber), IsUnique = true)]
public class CustomerReturn : DownloableP4WEntity
{
    public Guid? CustomerId { get; set; }
    public virtual Customer Customer { get; set; }

    public Guid WarehouseId { get; set; }
    public virtual Warehouse Warehouse { get; set; }

    [Required]
    [MaxLength(36)]
    public string CustomerReturnNumber { get; set; }

    public DateTime? DocumentDate { get; set; }

    public string ReferenceNumber { get; set; }

    public string Source { get; set; }
    public string LoopHash { get; set; }

    public string TrackingNumber { get; set; }
    public string Carrier { get; set; }
    public string Comments { get; set; }

    public string FromName { get; set; }
    public string FromEmail { get; set; }
    public string FromAddress1 { get; set; }
    public string FromAddress2 { get; set; }
    public string FromCity { get; set; }
    public string FromStateProvince { get; set; }
    public string FromZipPostal { get; set; }
    public string FromCountry { get; set; }
    public string FromPhone { get; set; }

    public string Info2 { get; set; }
    public string Info3 { get; set; }
    public string Info4 { get; set; }
    public string Info5 { get; set; }
    public string Info6 { get; set; }
    public string Info7 { get; set; }
    public string Info8 { get; set; }
    public string Info9 { get; set; }
    public string Info10 { get; set; }

    [ForeignKey(nameof(CustomerReturnLine.CustomerReturnId))]
    public virtual ICollection<CustomerReturnLine> Lines { get; set; } = new List<CustomerReturnLine>();
}

public class CustomerReturnLine : EntityBase
{
    public Guid CustomerReturnId { get; set; }
    public virtual CustomerReturn CustomerReturn { get; set; }

    public Guid ProductId { get; set; }
    public virtual Product Product { get; set; }

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

    public string ReferenceNumber { get; set; }

    public string Reference1 { get; set; }
    public string Reference2 { get; set; }
    public string Reference3 { get; set; }

    public int Quantity { get; set; }

    public int? ReceivedQuantity { get; set; }
    public int? DamagedQuantity { get; set; }

    public int? TotalReceived => ReceivedQuantity + DamagedQuantity;

    public string PickticketNumber { get; set; }
    public string PoNumber { get; set; }
}

public class CustomerReturnLineMap : EntityBaseMap<CustomerReturnLine>
{
    public override void Configure(EntityTypeBuilder<CustomerReturnLine> builder)
    {
        builder
            .HasOne(c => c.Product)
            .WithMany(c => c.CustomerReturnLines)
            .OnDelete(DeleteBehavior.NoAction);
    }
}