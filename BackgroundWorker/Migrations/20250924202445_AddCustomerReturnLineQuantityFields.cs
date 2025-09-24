using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pro4Soft.BackgroundWorker.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerReturnLineQuantityFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "ReceivedQuantity",
                table: "CustomerReturnLines",
                type: "decimal(15,6)",
                precision: 15,
                scale: 6,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "CustomerReturnLines",
                type: "decimal(15,6)",
                precision: 15,
                scale: 6,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "DamagedQuantity",
                table: "CustomerReturnLines",
                type: "decimal(15,6)",
                precision: 15,
                scale: 6,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ReceivedQuantity",
                table: "CustomerReturnLines",
                type: "int",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(15,6)",
                oldPrecision: 15,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "CustomerReturnLines",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(15,6)",
                oldPrecision: 15,
                oldScale: 6);

            migrationBuilder.AlterColumn<int>(
                name: "DamagedQuantity",
                table: "CustomerReturnLines",
                type: "int",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(15,6)",
                oldPrecision: 15,
                oldScale: 6,
                oldNullable: true);
        }
    }
}
