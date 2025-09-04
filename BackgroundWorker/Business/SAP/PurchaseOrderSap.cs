using System.Text.Json;

namespace Pro4Soft.BackgroundWorker.Business.SAP;

public class PurchaseOrderSap : BaseDocumentSap
{
    public string CardCode { get; set; }
    public string Comments { get; set; }

    public string Confirmed { get; set; }
    public string DocumentStatus { get; set; }

    public DateTime? UpdateDate { get; set; }
    public string UpdateTime { get; set; }
    public DateTime? ActualUpdated => UpdateDate?.Add(TimeSpan.Parse(UpdateTime ?? "00:00:00"));

    public AddressExtension AddressExtension { get; set; }
    public List<PurchaseOrderLineSap> DocumentLines { get; set; } = [];
}

public class AddressExtension
{
    public string ShipToStreet { get; set; }
    public string ShipToStreetNo { get; set; }
    public string ShipToBlock { get; set; }
    public string ShipToBuilding { get; set; }
    public string ShipToCity { get; set; }
    public string ShipToZipCode { get; set; }
    public string ShipToCounty { get; set; }
    public string ShipToState { get; set; }
    public string ShipToCountry { get; set; }
    public string ShipToAddressType { get; set; }

    public string BillToStreet { get; set; }
    public string BillToStreetNo { get; set; }
    public string BillToBlock { get; set; }
    public string BillToBuilding { get; set; }
    public string BillToCity { get; set; }
    public string BillToZipCode { get; set; }
    public string BillToCounty { get; set; }
    public string BillToState { get; set; }
    public string BillToCountry { get; set; }
    public string BillToAddressType { get; set; }

    public string ShipToGlobalLocationNumber { get; set; }
    public string BillToGlobalLocationNumber { get; set; }
    public string ShipToAddress2 { get; set; }
    public string ShipToAddress3 { get; set; }
    public string BillToAddress2 { get; set; }
    public string BillToAddress3 { get; set; }

    public string PlaceOfSupply { get; set; }
    public string PurchasePlaceOfSupply { get; set; }
    public int DocEntry { get; set; }

    public string GoodsIssuePlaceBP { get; set; }
    public string GoodsIssuePlaceCNPJ { get; set; }
    public string GoodsIssuePlaceCPF { get; set; }
    public string GoodsIssuePlaceStreet { get; set; }
    public string GoodsIssuePlaceStreetNo { get; set; }
    public string GoodsIssuePlaceBuilding { get; set; }
    public string GoodsIssuePlaceZip { get; set; }
    public string GoodsIssuePlaceBlock { get; set; }
    public string GoodsIssuePlaceCity { get; set; }
    public string GoodsIssuePlaceCounty { get; set; }
    public string GoodsIssuePlaceState { get; set; }
    public string GoodsIssuePlaceCountry { get; set; }
    public string GoodsIssuePlacePhone { get; set; }
    public string GoodsIssuePlaceEMail { get; set; }
    public string GoodsIssuePlaceDepartureDate { get; set; }

    public string DeliveryPlaceBP { get; set; }
    public string DeliveryPlaceCNPJ { get; set; }
    public string DeliveryPlaceCPF { get; set; }
    public string DeliveryPlaceStreet { get; set; }
    public string DeliveryPlaceStreetNo { get; set; }
    public string DeliveryPlaceBuilding { get; set; }
    public string DeliveryPlaceZip { get; set; }
    public string DeliveryPlaceBlock { get; set; }
    public string DeliveryPlaceCity { get; set; }
    public string DeliveryPlaceCounty { get; set; }
    public string DeliveryPlaceState { get; set; }
    public string DeliveryPlaceCountry { get; set; }
    public string DeliveryPlacePhone { get; set; }
    public string DeliveryPlaceEMail { get; set; }
    public string DeliveryPlaceDepartureDate { get; set; }
}

public class PurchaseOrderLineSap
{
    public int LineNum { get; set; }
    public string ItemCode { get; set; }
    public string ItemDescription { get; set; }
    public decimal? UnitsOfMeasurment { get; set; }
    public decimal RemainingOpenQuantity { get; set; }
    public string WarehouseCode { get; set; }
}