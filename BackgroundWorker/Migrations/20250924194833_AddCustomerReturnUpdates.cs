using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pro4Soft.BackgroundWorker.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerReturnUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsmanualCancelledClosed",
                table: "CustomerReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Reference1",
                table: "CustomerReturns",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Uploaded",
                table: "CustomerReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "VendorId",
                table: "CustomerReturns",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "LineNumber",
                table: "CustomerReturnLines",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfPacks",
                table: "CustomerReturnLines",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Packsize",
                table: "CustomerReturnLines",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CustomerReturnLineDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerReturnLineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceivedQuantity = table.Column<decimal>(type: "decimal(15,6)", precision: 15, scale: 6, nullable: false),
                    LotNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PacksizeEachCount = table.Column<int>(type: "int", nullable: true),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DateModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerReturnLineDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerReturnLineDetail_CustomerReturnLines_CustomerReturnLineId",
                        column: x => x.CustomerReturnLineId,
                        principalTable: "CustomerReturnLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerReturns_VendorId",
                table: "CustomerReturns",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerReturnLineDetail_CustomerReturnLineId",
                table: "CustomerReturnLineDetail",
                column: "CustomerReturnLineId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerReturns_Vendors_VendorId",
                table: "CustomerReturns",
                column: "VendorId",
                principalTable: "Vendors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerReturns_Vendors_VendorId",
                table: "CustomerReturns");

            migrationBuilder.DropTable(
                name: "CustomerReturnLineDetail");

            migrationBuilder.DropIndex(
                name: "IX_CustomerReturns_VendorId",
                table: "CustomerReturns");

            migrationBuilder.DropColumn(
                name: "IsmanualCancelledClosed",
                table: "CustomerReturns");

            migrationBuilder.DropColumn(
                name: "Reference1",
                table: "CustomerReturns");

            migrationBuilder.DropColumn(
                name: "Uploaded",
                table: "CustomerReturns");

            migrationBuilder.DropColumn(
                name: "VendorId",
                table: "CustomerReturns");

            migrationBuilder.DropColumn(
                name: "LineNumber",
                table: "CustomerReturnLines");

            migrationBuilder.DropColumn(
                name: "NumberOfPacks",
                table: "CustomerReturnLines");

            migrationBuilder.DropColumn(
                name: "Packsize",
                table: "CustomerReturnLines");
        }
    }
}
