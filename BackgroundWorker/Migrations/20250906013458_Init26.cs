using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pro4Soft.BackgroundWorker.Migrations
{
    /// <inheritdoc />
    public partial class Init26 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Adjustments_ProductId",
                table: "Adjustments",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Adjustments_Products_ProductId",
                table: "Adjustments",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Adjustments_Products_ProductId",
                table: "Adjustments");

            migrationBuilder.DropIndex(
                name: "IX_Adjustments_ProductId",
                table: "Adjustments");
        }
    }
}
