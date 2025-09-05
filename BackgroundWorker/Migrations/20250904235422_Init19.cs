using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pro4Soft.BackgroundWorker.Migrations
{
    /// <inheritdoc />
    public partial class Init19 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerProductDescription",
                table: "PickTicketLines");

            migrationBuilder.DropColumn(
                name: "ProductColor",
                table: "PickTicketLines");

            migrationBuilder.DropColumn(
                name: "ProductSize",
                table: "PickTicketLines");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "PickTicketLines",
                type: "decimal(15,6)",
                precision: 15,
                scale: 6,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "NumberOfPacks",
                table: "PickTicketLines",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Packsize",
                table: "PickTicketLines",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippedQuantity",
                table: "PickTicketLines",
                type: "decimal(15,6)",
                precision: 15,
                scale: 6,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumberOfPacks",
                table: "PickTicketLines");

            migrationBuilder.DropColumn(
                name: "Packsize",
                table: "PickTicketLines");

            migrationBuilder.DropColumn(
                name: "ShippedQuantity",
                table: "PickTicketLines");

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "PickTicketLines",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(15,6)",
                oldPrecision: 15,
                oldScale: 6);

            migrationBuilder.AddColumn<string>(
                name: "CustomerProductDescription",
                table: "PickTicketLines",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductColor",
                table: "PickTicketLines",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductSize",
                table: "PickTicketLines",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }
    }
}
