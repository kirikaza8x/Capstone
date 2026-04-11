using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Events.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameEventTypeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "event_type_id",
                schema: "events",
                table: "events");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "event_type_id",
                schema: "events",
                table: "events",
                type: "integer",
                nullable: true);
        }
    }
}
