using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pro4Soft.BackgroundWorker.Migrations
{
    /// <inheritdoc />
    public partial class Init14 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "PurchaseOrderLines",
                type: "decimal(15,6)",
                precision: 15,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(15,6)",
                oldPrecision: 15,
                oldScale: 6);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfPacks",
                table: "PurchaseOrderLines",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Packsize",
                table: "PurchaseOrderLines",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumberOfPacks",
                table: "PurchaseOrderLines");

            migrationBuilder.DropColumn(
                name: "Packsize",
                table: "PurchaseOrderLines");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "PurchaseOrderLines",
                type: "decimal(15,6)",
                precision: 15,
                scale: 6,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(15,6)",
                oldPrecision: 15,
                oldScale: 6,
                oldNullable: true);
        }
    }
}
