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
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerReturnLineDetail_CustomerReturnLines_CustomerReturnLineId",
                table: "CustomerReturnLineDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomerReturnLineDetail",
                table: "CustomerReturnLineDetail");

            migrationBuilder.RenameTable(
                name: "CustomerReturnLineDetail",
                newName: "CustomerReturnLineDetails");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerReturnLineDetail_CustomerReturnLineId",
                table: "CustomerReturnLineDetails",
                newName: "IX_CustomerReturnLineDetails_CustomerReturnLineId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomerReturnLineDetails",
                table: "CustomerReturnLineDetails",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerReturnLineDetails_CustomerReturnLines_CustomerReturnLineId",
                table: "CustomerReturnLineDetails",
                column: "CustomerReturnLineId",
                principalTable: "CustomerReturnLines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerReturnLineDetails_CustomerReturnLines_CustomerReturnLineId",
                table: "CustomerReturnLineDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomerReturnLineDetails",
                table: "CustomerReturnLineDetails");

            migrationBuilder.RenameTable(
                name: "CustomerReturnLineDetails",
                newName: "CustomerReturnLineDetail");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerReturnLineDetails_CustomerReturnLineId",
                table: "CustomerReturnLineDetail",
                newName: "IX_CustomerReturnLineDetail_CustomerReturnLineId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomerReturnLineDetail",
                table: "CustomerReturnLineDetail",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerReturnLineDetail_CustomerReturnLines_CustomerReturnLineId",
                table: "CustomerReturnLineDetail",
                column: "CustomerReturnLineId",
                principalTable: "CustomerReturnLines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
