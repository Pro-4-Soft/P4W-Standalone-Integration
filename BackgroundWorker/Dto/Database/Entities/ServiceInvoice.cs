using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Pro4Soft.BackgroundWorker.Dto.Database.Entities.Base;

namespace Pro4Soft.BackgroundWorker.Dto.Database.Entities;

[Index(nameof(InvoiceNumber), IsUnique = true)]
public class ServiceInvoice : P4WStateEntity
{
    [Required]
    [MaxLength(64)]
    public string InvoiceNumber { get; set; }

    public Guid ClientId { get; set; }

    [MaxLength(256)]
    public string CustomerCode { get; set; }

    public DateTime? StartPeriod { get; set; }
    public DateTime? EndPeriod { get; set; }
    public DateTime? PostingDate { get; set; }

    public decimal? SubTotal { get; set; }
    public decimal? Total { get; set; }

    public bool Uploaded { get; set; }

    [ForeignKey(nameof(ServiceInvoiceLine.ServiceInvoiceId))]
    public virtual ICollection<ServiceInvoiceLine> Lines { get; set; } = new List<ServiceInvoiceLine>();
}

public class ServiceInvoiceLine : EntityBase
{
    public Guid ServiceInvoiceId { get; set; }
    public virtual ServiceInvoice ServiceInvoice { get; set; }

    // Product code from P4W (item)
    [MaxLength(64)]
    public string Item { get; set; }

    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
}

public class ServiceInvoiceLineMap : EntityBaseMap<ServiceInvoiceLine>
{
    public override void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<ServiceInvoiceLine> builder)
    {
        // no special mapping required for now
    }
}
