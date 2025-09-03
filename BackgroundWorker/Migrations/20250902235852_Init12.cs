using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pro4Soft.BackgroundWorker.Migrations
{
    /// <inheritdoc />
    public partial class Init12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "ReceivedQuantity",
                table: "PurchaseOrderLines",
                type: "decimal(15,6)",
                precision: 15,
                scale: 6,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "PurchaseOrderLines",
                type: "decimal(15,6)",
                precision: 15,
                scale: 6,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "PurchaseOrderLines",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "PurchaseOrderLines");

            migrationBuilder.AlterColumn<int>(
                name: "ReceivedQuantity",
                table: "PurchaseOrderLines",
                type: "int",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(15,6)",
                oldPrecision: 15,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "PurchaseOrderLines",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(15,6)",
                oldPrecision: 15,
                oldScale: 6);
        }
    }
}
