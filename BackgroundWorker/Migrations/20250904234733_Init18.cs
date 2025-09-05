using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pro4Soft.BackgroundWorker.Migrations
{
    /// <inheritdoc />
    public partial class Init18 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Reference1",
                table: "PickTickets",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reference2",
                table: "PickTickets",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reference3",
                table: "PickTickets",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Uploaded",
                table: "PickTickets",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reference1",
                table: "PickTickets");

            migrationBuilder.DropColumn(
                name: "Reference2",
                table: "PickTickets");

            migrationBuilder.DropColumn(
                name: "Reference3",
                table: "PickTickets");

            migrationBuilder.DropColumn(
                name: "Uploaded",
                table: "PickTickets");
        }
    }
}
