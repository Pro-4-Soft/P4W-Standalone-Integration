using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pro4Soft.BackgroundWorker.Migrations
{
    /// <inheritdoc />
    public partial class Init27 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "Adjustments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "P4WId",
                table: "Adjustments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Adjustments",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Adjustments_P4WId",
                table: "Adjustments",
                column: "P4WId");

            migrationBuilder.CreateIndex(
                name: "IX_Adjustments_State",
                table: "Adjustments",
                column: "State");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Adjustments_P4WId",
                table: "Adjustments");

            migrationBuilder.DropIndex(
                name: "IX_Adjustments_State",
                table: "Adjustments");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "Adjustments");

            migrationBuilder.DropColumn(
                name: "P4WId",
                table: "Adjustments");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Adjustments");
        }
    }
}
