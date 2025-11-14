using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pro4Soft.BackgroundWorker.Dto.Database.Entities.Base;

namespace Pro4Soft.BackgroundWorker.Dto.Database.Entities;

[Index(nameof(PickTicketNumber), IsUnique = true)]
public class Pickticket : P4WStateEntity
{
    public Guid CustomerId { get; set; }
    public virtual Customer Customer { get; set; }

    public string WarehouseCode { get; set; }

    [Required]
    [MaxLength(36)]
    public string PickTicketNumber { get; set; }

    public string PoNumber { get; set; }

    public string RouteNumber { get; set; }

    public string referenceNumber { get; set; }
    public string PickingInstructions { get; set; }
    public string Comments { get; set; }

    public string ShipFromName { get; set; }
    public string ShipFromPhone { get; set; }
    public string ShipFromEmail { get; set; }
    public string ShipFromAddress1 { get; set; }
    public string ShipFromAddress2 { get; set; }
    public string ShipFromCity { get; set; }
    public string ShipFromStateProvince { get; set; }
    public string ShipFromZipPostal { get; set; }
    public string ShipFromCountry { get; set; }

    public string BillToName { get; set; }
    public string BillToPhone { get; set; }
    public string BillToEmail { get; set; }
    public string BillToAddress1 { get; set; }
    public string BillToAddress2 { get; set; }
    public string BillToCity { get; set; }
    public string BillToStateProvince { get; set; }
    public string BillToZipPostal { get; set; }
    public string BillToCountry { get; set; }

    public string ShipToCode { get; set; }
    public string ShipToName { get; set; }
    public string ShipToPhone { get; set; }
    public string ShipToEmail { get; set; }
    public string ShipToAddress1 { get; set; }
    public string ShipToAddress2 { get; set; }
    public string ShipToCity { get; set; }
    public string ShipToStateProvince { get; set; }
    public string ShipToZipPostal { get; set; }
    public string ShipToCountry { get; set; }

    public string DepartmentName { get; set; }
    public string DepartmentNumber { get; set; }
    public string DepartmentDescription { get; set; }
    public string VendorName { get; set; }
    public string VendorCode { get; set; }
    public string SupplierNumber { get; set; }
    public string ArticleNumber { get; set; }
    public string MarkForStoreName { get; set; }
    public string MarkForStoreNumber { get; set; }
    public string StoreNumber { get; set; }
    public string CustomerReferenceNumber { get; set; }
    public decimal? OrderTotal { get; set; }

    public string Reference1 { get; set; }
    public string Reference2 { get; set; }
    public string Reference3 { get; set; }

    public string Info2 { get; set; }
    public string Info3 { get; set; }
    public string Info4 { get; set; }
    public string Info5 { get; set; }
    public string Info6 { get; set; }
    public string Info7 { get; set; }
    public string Info8 { get; set; }
    public string Info9 { get; set; }
    public string Info10 { get; set; }
               
    public DateTime? CancelDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public DateTime? CloseDate { get; set; }
    public DateTime? MustArriveDate { get; set; }

    public bool IsManualCancelledClosed { get; set; }

    public PaymentType PaymentType { get; set; }

    public string Carrier { get; set; }
    public string ShippingService { get; set; }

    public string FileDownloadPath { get; set; }
    public FreightType FreightType { get; set; } = FreightType.PrivateFleet;

    public bool Uploaded { get; set; }

    [ForeignKey(nameof(PickTicketLine.PickTicketId))]
    public virtual ICollection<PickTicketLine> Lines { get; set; } = new List<PickTicketLine>();

    [ForeignKey(nameof(Tote.PickTicketId))]
    public virtual ICollection<Tote> Totes { get; set; } = new List<Tote>();
}

public class PickTicketLine : EntityBase
{
    [Required]
    public Guid PickTicketId { get; set; }
    public virtual Pickticket Pickticket { get; set; }

    [Required]
    public Guid ProductId { get; set; }
    public virtual Product Product { get; set; }

    public int LineNumber { get; set; }

    public int? Packsize { get; set; }
    public int? NumberOfPacks { get; set; }

    public decimal Quantity { get; set; }

    public decimal? SalesPrice { get; set; }

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

    [ForeignKey(nameof(ToteLine.PickTicketLineId))]
    public virtual ICollection<ToteLine> ToteLines { get; set; } = new List<ToteLine>();
}

public class Tote : EntityBase
{
    public Guid P4WId { get; set; }

    [Required]
    public Guid PickTicketId { get; set; }
    public virtual Pickticket Pickticket { get; set; }

    public string Sscc18Code { get; set; }
    public string PalletSscc18Code { get; set; }
    public string SealNumber { get; set; }
    public string LpnCode { get; set; }

    public int CartonNumber { get; set; }
    public string Carrier { get; set; }
    public string ScacCode { get; set; }
    public string TrailerNumber { get; set; }
    public string TrackTraceNumber { get; set; }
    public string ShippingService { get; set; }
    public decimal? ShippingCost { get; set; }

    public string ProNumber { get; set; }
    public string BolNumber { get; set; }
    public string MasterBolNumber { get; set; }

    public decimal? Weight { get; set; }
    public decimal? Length { get; set; }
    public decimal? Height { get; set; }
    public decimal? Width { get; set; }

    public string TruckLoadInfo1 { get; set; }
    public string TruckLoadInfo2 { get; set; }
    public string TruckLoadInfo3 { get; set; }

    [ForeignKey(nameof(ToteLine.ToteId))]
    public virtual ICollection<ToteLine> Lines { get; set; } = new List<ToteLine>();
}

public class ToteLine : EntityBase
{
    public Guid P4WId { get; set; }

    public Guid ToteId { get; set; }
    public virtual Tote Tote { get; set; }

    public decimal ShippedQuantity { get; set; }

    public Guid PickTicketLineId { get; set; }
    public virtual PickTicketLine PickTicketLine { get; set; }

    [ForeignKey(nameof(ToteLineDetail.ToteLineId))]
    public virtual ICollection<ToteLineDetail> Details { get; set; } = new List<ToteLineDetail>();
}

public class ToteLineDetail : EntityBase
{
    public Guid P4WId { get; set; }

    public Guid ToteLineId { get; set; }
    public virtual ToteLine ToteLine { get; set; }

    public decimal ShippedQuantity { get; set; }
    public string LotNumber { get; set; }
    public string SerialNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int? PacksizeEachCount { get; set; }
}

public class ToteLineMap : EntityBaseMap<ToteLine>
{
    public override void Configure(EntityTypeBuilder<ToteLine> builder)
    {
        builder
            .HasOne(c => c.PickTicketLine)
            .WithMany(c => c.ToteLines)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public enum FreightType
{
    PrivateFleet,
    TruckLoad,
    SmallParcel,
    External
}

public enum PaymentType
{
    ThirdParty, 
    Collect, 
    PrePay
}