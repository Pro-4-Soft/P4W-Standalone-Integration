using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pro4Soft.BackgroundWorker.Migrations
{
    /// <inheritdoc />
    public partial class Init23 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CartonName",
                table: "Totes");

            migrationBuilder.DropColumn(
                name: "ShippedQuantity",
                table: "PickTicketLines");

            migrationBuilder.AddColumn<Guid>(
                name: "P4WId",
                table: "Totes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<decimal>(
                name: "ShippedQuantity",
                table: "ToteLines",
                type: "decimal(15,6)",
                precision: 15,
                scale: 6,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<Guid>(
                name: "P4WId",
                table: "ToteLines",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ToteLineDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    P4WId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToteLineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShippedQuantity = table.Column<decimal>(type: "decimal(15,6)", precision: 15, scale: 6, nullable: false),
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
                    table.PrimaryKey("PK_ToteLineDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ToteLineDetails_ToteLines_ToteLineId",
                        column: x => x.ToteLineId,
                        principalTable: "ToteLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ToteLineDetails_ToteLineId",
                table: "ToteLineDetails",
                column: "ToteLineId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ToteLineDetails");

            migrationBuilder.DropColumn(
                name: "P4WId",
                table: "Totes");

            migrationBuilder.DropColumn(
                name: "P4WId",
                table: "ToteLines");

            migrationBuilder.AddColumn<string>(
                name: "CartonName",
                table: "Totes",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ShippedQuantity",
                table: "ToteLines",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(15,6)",
                oldPrecision: 15,
                oldScale: 6);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippedQuantity",
                table: "PickTicketLines",
                type: "decimal(15,6)",
                precision: 15,
                scale: 6,
                nullable: true);
        }
    }
}
