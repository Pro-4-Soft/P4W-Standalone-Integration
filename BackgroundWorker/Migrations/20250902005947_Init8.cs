using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pro4Soft.BackgroundWorker.Migrations
{
    /// <inheritdoc />
    public partial class Init8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColorDescription",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "IsPacksizeController",
                table: "Products",
                newName: "IsSerialControlled");

            migrationBuilder.RenameColumn(
                name: "Customer",
                table: "Products",
                newName: "Category");

            migrationBuilder.AddColumn<bool>(
                name: "IsLotControlled",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPacksizeControlled",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLotControlled",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsPacksizeControlled",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "IsSerialControlled",
                table: "Products",
                newName: "IsPacksizeController");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "Products",
                newName: "Customer");

            migrationBuilder.AddColumn<string>(
                name: "ColorDescription",
                table: "Products",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }
    }
}
