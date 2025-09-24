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
                name: "LoopHash",
                table: "CustomerReturns");

            migrationBuilder.RenameColumn(
                name: "IsmanualCancelledClosed",
                table: "PurchaseOrders",
                newName: "IsManualCancelledClosed");

            migrationBuilder.RenameColumn(
                name: "IsmanualCancelledClosed",
                table: "CustomerReturns",
                newName: "IsManualCancelledClosed");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsManualCancelledClosed",
                table: "PurchaseOrders",
                newName: "IsmanualCancelledClosed");

            migrationBuilder.RenameColumn(
                name: "IsManualCancelledClosed",
                table: "CustomerReturns",
                newName: "IsmanualCancelledClosed");

            migrationBuilder.AddColumn<string>(
                name: "LoopHash",
                table: "CustomerReturns",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }
    }
}
