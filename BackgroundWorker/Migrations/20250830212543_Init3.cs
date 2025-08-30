using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pro4Soft.BackgroundWorker.Migrations
{
    /// <inheritdoc />
    public partial class Init3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LineNumber",
                table: "PurchaseOrderLines",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LineNumber",
                table: "PurchaseOrderLines");
        }
    }
}
