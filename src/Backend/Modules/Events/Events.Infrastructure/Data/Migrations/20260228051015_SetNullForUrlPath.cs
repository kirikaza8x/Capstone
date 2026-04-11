using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Events.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SetNullForUrlPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_events_url_path",
                schema: "events",
                table: "events");

            migrationBuilder.AlterColumn<string>(
                name: "url_path",
                schema: "events",
                table: "events",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.CreateIndex(
                name: "ix_events_url_path",
                schema: "events",
                table: "events",
                column: "url_path",
                unique: true,
                filter: "\"url_path\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_events_url_path",
                schema: "events",
                table: "events");

            migrationBuilder.AlterColumn<string>(
                name: "url_path",
                schema: "events",
                table: "events",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_events_url_path",
                schema: "events",
                table: "events",
                column: "url_path",
                unique: true);
        }
    }
}
