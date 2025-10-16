using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pro4Soft.BackgroundWorker.Migrations
{
    /// <inheritdoc />
    public partial class ReferenceNumberRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "referenceNumber",
                table: "PickTickets",
                newName: "ReferenceNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReferenceNumber",
                table: "PickTickets",
                newName: "referenceNumber");
        }
    }
}
