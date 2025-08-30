using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pro4Soft.BackgroundWorker.Migrations
{
    /// <inheritdoc />
    public partial class Init2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Division",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "WarehouseCode",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "Division",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PackType",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Division",
                table: "PickTickets");

            migrationBuilder.DropColumn(
                name: "WarehouseCode",
                table: "PickTickets");

            migrationBuilder.DropColumn(
                name: "Division",
                table: "CustomerReturns");

            migrationBuilder.DropColumn(
                name: "WarehouseCode",
                table: "CustomerReturns");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Vendors",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "Division",
                table: "Vendors",
                newName: "CompanyName");

            migrationBuilder.RenameColumn(
                name: "Season",
                table: "Products",
                newName: "Info4");

            migrationBuilder.RenameColumn(
                name: "HtsTariffNumber",
                table: "Products",
                newName: "Info3");

            migrationBuilder.RenameColumn(
                name: "ColorCode",
                table: "Products",
                newName: "Info2");

            migrationBuilder.RenameColumn(
                name: "Eaches",
                table: "Packsizes",
                newName: "EachCount");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Customers",
                newName: "CompanyName");

            migrationBuilder.AlterColumn<string>(
                name: "DownloadError",
                table: "Vendors",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DownloadError",
                table: "PurchaseOrders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WarehouseId",
                table: "PurchaseOrders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "DownloadError",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HtsNumber",
                table: "Products",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Info1",
                table: "Products",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPacksizeController",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "DownloadError",
                table: "PickTickets",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WarehouseId",
                table: "PickTickets",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "DownloadError",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DownloadError",
                table: "CustomerReturns",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WarehouseId",
                table: "CustomerReturns",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Warehouses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DateModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    P4WId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_WarehouseId",
                table: "PurchaseOrders",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_PickTickets_WarehouseId",
                table: "PickTickets",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerReturns_WarehouseId",
                table: "CustomerReturns",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_P4WId",
                table: "Warehouses",
                column: "P4WId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerReturns_Warehouses_WarehouseId",
                table: "CustomerReturns",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PickTickets_Warehouses_WarehouseId",
                table: "PickTickets",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Warehouses_WarehouseId",
                table: "PurchaseOrders",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerReturns_Warehouses_WarehouseId",
                table: "CustomerReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_PickTickets_Warehouses_WarehouseId",
                table: "PickTickets");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Warehouses_WarehouseId",
                table: "PurchaseOrders");

            migrationBuilder.DropTable(
                name: "Warehouses");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_WarehouseId",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_PickTickets_WarehouseId",
                table: "PickTickets");

            migrationBuilder.DropIndex(
                name: "IX_CustomerReturns_WarehouseId",
                table: "CustomerReturns");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "HtsNumber",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Info1",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsPacksizeController",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "PickTickets");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "CustomerReturns");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Vendors",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "CompanyName",
                table: "Vendors",
                newName: "Division");

            migrationBuilder.RenameColumn(
                name: "Info4",
                table: "Products",
                newName: "Season");

            migrationBuilder.RenameColumn(
                name: "Info3",
                table: "Products",
                newName: "HtsTariffNumber");

            migrationBuilder.RenameColumn(
                name: "Info2",
                table: "Products",
                newName: "ColorCode");

            migrationBuilder.RenameColumn(
                name: "EachCount",
                table: "Packsizes",
                newName: "Eaches");

            migrationBuilder.RenameColumn(
                name: "CompanyName",
                table: "Customers",
                newName: "Name");

            migrationBuilder.AlterColumn<string>(
                name: "DownloadError",
                table: "Vendors",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DownloadError",
                table: "PurchaseOrders",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Division",
                table: "PurchaseOrders",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WarehouseCode",
                table: "PurchaseOrders",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "DownloadError",
                table: "Products",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Division",
                table: "Products",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PackType",
                table: "Products",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "DownloadError",
                table: "PickTickets",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Division",
                table: "PickTickets",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WarehouseCode",
                table: "PickTickets",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "DownloadError",
                table: "Customers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DownloadError",
                table: "CustomerReturns",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Division",
                table: "CustomerReturns",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WarehouseCode",
                table: "CustomerReturns",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }
    }
}
