using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pro4Soft.BackgroundWorker.Migrations
{
    /// <inheritdoc />
    public partial class Init13 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DownloadError",
                table: "Vendors",
                newName: "ErrorMessage");

            migrationBuilder.RenameColumn(
                name: "DownloadError",
                table: "PurchaseOrders",
                newName: "ErrorMessage");

            migrationBuilder.RenameColumn(
                name: "DownloadError",
                table: "Products",
                newName: "ErrorMessage");

            migrationBuilder.RenameColumn(
                name: "DownloadError",
                table: "PickTickets",
                newName: "ErrorMessage");

            migrationBuilder.RenameColumn(
                name: "DownloadError",
                table: "Customers",
                newName: "ErrorMessage");

            migrationBuilder.RenameColumn(
                name: "DownloadError",
                table: "CustomerReturns",
                newName: "ErrorMessage");

            migrationBuilder.AddColumn<string>(
                name: "Reference1",
                table: "PurchaseOrders",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsExpiryControlled",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "PurchaseOrderLineDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseOrderLineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceivedQuantity = table.Column<decimal>(type: "decimal(15,6)", precision: 15, scale: 6, nullable: false),
                    LotNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PacksizeEachCount = table.Column<int>(type: "int", nullable: true),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DateModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderLineDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderLineDetails_PurchaseOrderLines_PurchaseOrderLineId",
                        column: x => x.PurchaseOrderLineId,
                        principalTable: "PurchaseOrderLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderLineDetails_PurchaseOrderLineId",
                table: "PurchaseOrderLineDetails",
                column: "PurchaseOrderLineId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PurchaseOrderLineDetails");

            migrationBuilder.DropColumn(
                name: "Reference1",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "IsExpiryControlled",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "ErrorMessage",
                table: "Vendors",
                newName: "DownloadError");

            migrationBuilder.RenameColumn(
                name: "ErrorMessage",
                table: "PurchaseOrders",
                newName: "DownloadError");

            migrationBuilder.RenameColumn(
                name: "ErrorMessage",
                table: "Products",
                newName: "DownloadError");

            migrationBuilder.RenameColumn(
                name: "ErrorMessage",
                table: "PickTickets",
                newName: "DownloadError");

            migrationBuilder.RenameColumn(
                name: "ErrorMessage",
                table: "Customers",
                newName: "DownloadError");

            migrationBuilder.RenameColumn(
                name: "ErrorMessage",
                table: "CustomerReturns",
                newName: "DownloadError");
        }
    }
}
