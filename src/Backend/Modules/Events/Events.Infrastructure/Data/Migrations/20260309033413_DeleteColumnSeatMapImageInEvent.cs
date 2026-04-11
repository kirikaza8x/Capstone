using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Events.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DeleteColumnSeatMapImageInEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "seatmap_image",
                schema: "events",
                table: "events");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "seatmap_image",
                schema: "events",
                table: "events",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
