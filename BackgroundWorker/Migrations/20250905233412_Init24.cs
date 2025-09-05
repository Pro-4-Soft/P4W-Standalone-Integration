using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pro4Soft.BackgroundWorker.Migrations
{
    /// <inheritdoc />
    public partial class Init24 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Barcode",
                table: "PickTickets");

            migrationBuilder.DropColumn(
                name: "DataQuery",
                table: "PickTickets");

            migrationBuilder.DropColumn(
                name: "FedexAuthenticationAccountNumber",
                table: "PickTickets");

            migrationBuilder.DropColumn(
                name: "IsInternational",
                table: "PickTickets");

            migrationBuilder.DropColumn(
                name: "IsResidential",
                table: "PickTickets");

            migrationBuilder.DropColumn(
                name: "IsSignatureRequired",
                table: "PickTickets");

            migrationBuilder.DropColumn(
                name: "OrderTotalValue",
                table: "PickTickets");

            migrationBuilder.DropColumn(
                name: "PurchaseOrderFacilityCode",
                table: "PickTickets");

            migrationBuilder.DropColumn(
                name: "ShipCode",
                table: "PickTickets");

            migrationBuilder.DropColumn(
                name: "ThirdPartyAccountNumber",
                table: "PickTickets");

            migrationBuilder.DropColumn(
                name: "ThirdPartyCountry",
                table: "PickTickets");

            migrationBuilder.DropColumn(
                name: "ThirdPartyPostalCode",
                table: "PickTickets");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Barcode",
                table: "PickTickets",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataQuery",
                table: "PickTickets",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FedexAuthenticationAccountNumber",
                table: "PickTickets",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsInternational",
                table: "PickTickets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsResidential",
                table: "PickTickets",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSignatureRequired",
                table: "PickTickets",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OrderTotalValue",
                table: "PickTickets",
                type: "decimal(15,6)",
                precision: 15,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PurchaseOrderFacilityCode",
                table: "PickTickets",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipCode",
                table: "PickTickets",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThirdPartyAccountNumber",
                table: "PickTickets",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThirdPartyCountry",
                table: "PickTickets",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThirdPartyPostalCode",
                table: "PickTickets",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }
    }
}
