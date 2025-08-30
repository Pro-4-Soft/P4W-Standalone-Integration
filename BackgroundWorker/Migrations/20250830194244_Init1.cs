using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pro4Soft.BackgroundWorker.Migrations
{
    /// <inheritdoc />
    public partial class Init1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SsccCompanyId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DateModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    P4WId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ContactPerson = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info1 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info2 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info3 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info4 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info5 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info6 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info7 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info8 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info9 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info10 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DateModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    P4WId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    State = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DownloadError = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customers_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Sku = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    Division = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Upc = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Nmfc = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CommodityDescription = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    FreightClass = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PackType = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(15,6)", precision: 15, scale: 6, nullable: true),
                    Height = table.Column<decimal>(type: "decimal(15,6)", precision: 15, scale: 6, nullable: true),
                    Width = table.Column<decimal>(type: "decimal(15,6)", precision: 15, scale: 6, nullable: true),
                    Length = table.Column<decimal>(type: "decimal(15,6)", precision: 15, scale: 6, nullable: true),
                    Season = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ColorCode = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ColorDescription = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Customer = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info6 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info7 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info8 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info9 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info10 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    HtsTariffNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CountryOfOrigin = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DateModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    P4WId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    State = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DownloadError = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vendors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Division = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info2 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info3 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info4 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info5 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info6 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info7 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info8 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info9 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info10 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DateModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    P4WId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    State = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DownloadError = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vendors_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerReturns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerReturnNumber = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    WarehouseCode = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Division = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DocumentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Source = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LoopHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    TrackingNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Carrier = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    FromName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    FromEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    FromAddress1 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    FromAddress2 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    FromCity = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    FromStateProvince = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    FromZipPostal = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    FromCountry = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    FromPhone = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info2 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info3 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info4 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info5 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info6 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info7 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info8 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info9 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info10 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DateModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    P4WId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    State = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DownloadError = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerReturns_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PickTickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PickTicketNumber = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    WarehouseCode = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Division = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PoNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    RouteNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PickingInstructions = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ShipFromName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ShipFromPhone = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ShipFromAddress1 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ShipFromAddress2 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ShipFromCity = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ShipFromStateProvince = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ShipFromZipPostal = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ShipFromCountry = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    BillToName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    BillToPhone = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    BillToAddress1 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    BillToAddress2 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    BillToCity = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    BillToStateProvince = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    BillToZipPostal = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    BillToCountry = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ShipToCode = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ShipToName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ShipToPhone = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ShipToEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ShipToAddress1 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ShipToAddress2 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ShipToCity = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ShipToStateProvince = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ShipToZipPostal = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ShipToCountry = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DepartmentName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DepartmentNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DepartmentDescription = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    VendorName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    VendorCode = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    SupplierNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ArticleNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    MarkForStoreName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    MarkForStoreNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    StoreNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CustomerReferenceNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    OrderTotal = table.Column<decimal>(type: "decimal(15,6)", precision: 15, scale: 6, nullable: true),
                    Info2 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info3 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info4 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info5 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info6 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info7 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info8 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info9 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info10 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    OrderTotalValue = table.Column<decimal>(type: "decimal(15,6)", precision: 15, scale: 6, nullable: true),
                    Barcode = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PurchaseOrderFacilityCode = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CancelDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RequiredDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CloseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MustArriveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ShipCode = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DataQuery = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    FedexAuthenticationAccountNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PaymentType = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Carrier = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ShippingService = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ThirdPartyAccountNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ThirdPartyPostalCode = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ThirdPartyCountry = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsInternational = table.Column<bool>(type: "bit", nullable: false),
                    IsResidential = table.Column<bool>(type: "bit", nullable: true),
                    IsSignatureRequired = table.Column<bool>(type: "bit", nullable: true),
                    FreightType = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DateModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    P4WId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    State = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DownloadError = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PickTickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PickTickets_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Packsizes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(15,6)", precision: 15, scale: 6, nullable: true),
                    Height = table.Column<decimal>(type: "decimal(15,6)", precision: 15, scale: 6, nullable: true),
                    Width = table.Column<decimal>(type: "decimal(15,6)", precision: 15, scale: 6, nullable: true),
                    Length = table.Column<decimal>(type: "decimal(15,6)", precision: 15, scale: 6, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Eaches = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DateModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packsizes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Packsizes_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VendorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseOrderNumber = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    WarehouseCode = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Division = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    RequiredDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Container = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Carrier = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    AppointmentNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info2 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info3 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info4 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info5 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info6 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info7 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info8 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info9 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info10 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DateModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    P4WId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    State = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DownloadError = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerReturnLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerReturnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Info1 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info2 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info3 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info4 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info5 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info6 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info7 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info8 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info9 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info10 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Reference1 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Reference2 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Reference3 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ReceivedQuantity = table.Column<int>(type: "int", nullable: true),
                    DamagedQuantity = table.Column<int>(type: "int", nullable: true),
                    PickticketNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PoNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DateModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerReturnLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerReturnLines_CustomerReturns_CustomerReturnId",
                        column: x => x.CustomerReturnId,
                        principalTable: "CustomerReturns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerReturnLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PickTicketLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PickTicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LineNumber = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    SalesPrice = table.Column<decimal>(type: "decimal(15,6)", precision: 15, scale: 6, nullable: true),
                    ProductSize = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ProductColor = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CustomerProductDescription = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info1 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info2 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info3 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info4 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info5 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info6 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info7 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info8 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info9 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info10 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DateModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PickTicketLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PickTicketLines_PickTickets_PickTicketId",
                        column: x => x.PickTicketId,
                        principalTable: "PickTickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PickTicketLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Totes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PickTicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Sscc18Code = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PalletSscc18Code = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    SealNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LpnCode = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CartonNumber = table.Column<int>(type: "int", nullable: false),
                    Carrier = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ScacCode = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    TrailerNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    TrackTraceNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ShippingService = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ShippingCost = table.Column<decimal>(type: "decimal(15,6)", precision: 15, scale: 6, nullable: true),
                    ProNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    BolNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    MasterBolNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CartonName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(15,6)", precision: 15, scale: 6, nullable: true),
                    Length = table.Column<decimal>(type: "decimal(15,6)", precision: 15, scale: 6, nullable: true),
                    Height = table.Column<decimal>(type: "decimal(15,6)", precision: 15, scale: 6, nullable: true),
                    Width = table.Column<decimal>(type: "decimal(15,6)", precision: 15, scale: 6, nullable: true),
                    TruckLoadInfo1 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    TruckLoadInfo2 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    TruckLoadInfo3 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DateModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Totes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Totes_PickTickets_PickTicketId",
                        column: x => x.PickTicketId,
                        principalTable: "PickTickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Info1 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info2 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info3 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info4 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info5 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info6 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info7 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info8 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info9 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Info10 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ReceivedQuantity = table.Column<int>(type: "int", nullable: true),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DateModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseOrderLines_PurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ToteLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShippedQuantity = table.Column<int>(type: "int", nullable: false),
                    PickTicketLineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DateModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToteLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ToteLines_PickTicketLines_PickTicketLineId",
                        column: x => x.PickTicketLineId,
                        principalTable: "PickTicketLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ToteLines_Totes_ToteId",
                        column: x => x.ToteId,
                        principalTable: "Totes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_P4WId",
                table: "Clients",
                column: "P4WId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerReturnLines_CustomerReturnId",
                table: "CustomerReturnLines",
                column: "CustomerReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerReturnLines_ProductId",
                table: "CustomerReturnLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerReturns_CustomerId",
                table: "CustomerReturns",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerReturns_CustomerReturnNumber",
                table: "CustomerReturns",
                column: "CustomerReturnNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerReturns_P4WId",
                table: "CustomerReturns",
                column: "P4WId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerReturns_State",
                table: "CustomerReturns",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_ClientId",
                table: "Customers",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_P4WId",
                table: "Customers",
                column: "P4WId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_State",
                table: "Customers",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_Packsizes_ProductId",
                table: "Packsizes",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PickTicketLines_PickTicketId",
                table: "PickTicketLines",
                column: "PickTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_PickTicketLines_ProductId",
                table: "PickTicketLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PickTickets_CustomerId",
                table: "PickTickets",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_PickTickets_P4WId",
                table: "PickTickets",
                column: "P4WId");

            migrationBuilder.CreateIndex(
                name: "IX_PickTickets_PickTicketNumber",
                table: "PickTickets",
                column: "PickTicketNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PickTickets_State",
                table: "PickTickets",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ClientId",
                table: "Products",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_P4WId",
                table: "Products",
                column: "P4WId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Sku",
                table: "Products",
                column: "Sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_State",
                table: "Products",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderLines_ProductId",
                table: "PurchaseOrderLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderLines_PurchaseOrderId",
                table: "PurchaseOrderLines",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_P4WId",
                table: "PurchaseOrders",
                column: "P4WId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_PurchaseOrderNumber",
                table: "PurchaseOrders",
                column: "PurchaseOrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_State",
                table: "PurchaseOrders",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_VendorId",
                table: "PurchaseOrders",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_ToteLines_PickTicketLineId",
                table: "ToteLines",
                column: "PickTicketLineId");

            migrationBuilder.CreateIndex(
                name: "IX_ToteLines_ToteId",
                table: "ToteLines",
                column: "ToteId");

            migrationBuilder.CreateIndex(
                name: "IX_Totes_PickTicketId",
                table: "Totes",
                column: "PickTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_ClientId",
                table: "Vendors",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_P4WId",
                table: "Vendors",
                column: "P4WId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_State",
                table: "Vendors",
                column: "State");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerReturnLines");

            migrationBuilder.DropTable(
                name: "Packsizes");

            migrationBuilder.DropTable(
                name: "PurchaseOrderLines");

            migrationBuilder.DropTable(
                name: "ToteLines");

            migrationBuilder.DropTable(
                name: "CustomerReturns");

            migrationBuilder.DropTable(
                name: "PurchaseOrders");

            migrationBuilder.DropTable(
                name: "PickTicketLines");

            migrationBuilder.DropTable(
                name: "Totes");

            migrationBuilder.DropTable(
                name: "Vendors");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "PickTickets");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Clients");
        }
    }
}
