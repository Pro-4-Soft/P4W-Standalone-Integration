using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pro4Soft.BackgroundWorker.Migrations
{
    /// <inheritdoc />
    public partial class RemoveVendorFromCustomerReturn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerReturns_Vendors_VendorId",
                table: "CustomerReturns");

            migrationBuilder.DropIndex(
                name: "IX_CustomerReturns_VendorId",
                table: "CustomerReturns");

            migrationBuilder.DropColumn(
                name: "VendorId",
                table: "CustomerReturns");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "VendorId",
                table: "CustomerReturns",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_CustomerReturns_VendorId",
                table: "CustomerReturns",
                column: "VendorId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerReturns_Vendors_VendorId",
                table: "CustomerReturns",
                column: "VendorId",
                principalTable: "Vendors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
