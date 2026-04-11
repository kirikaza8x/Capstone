using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Users.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class update_organizer_field_configuration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_organizerprofile_user_id",
                schema: "users",
                table: "OrganizerProfile");

            migrationBuilder.RenameTable(
                name: "OrganizerProfile",
                schema: "users",
                newName: "organizer_profile",
                newSchema: "users");

            migrationBuilder.AddColumn<string>(
                name: "reject_reason",
                schema: "users",
                table: "organizer_profile",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "version_number",
                schema: "users",
                table: "organizer_profile",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_organizerprofile_user_status",
                schema: "users",
                table: "organizer_profile",
                columns: new[] { "user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_organizerprofile_user_version",
                schema: "users",
                table: "organizer_profile",
                columns: new[] { "user_id", "version_number" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_organizerprofile_user_status",
                schema: "users",
                table: "organizer_profile");

            migrationBuilder.DropIndex(
                name: "ix_organizerprofile_user_version",
                schema: "users",
                table: "organizer_profile");

            migrationBuilder.DropColumn(
                name: "reject_reason",
                schema: "users",
                table: "organizer_profile");

            migrationBuilder.DropColumn(
                name: "version_number",
                schema: "users",
                table: "organizer_profile");

            migrationBuilder.RenameTable(
                name: "organizer_profile",
                schema: "users",
                newName: "OrganizerProfile",
                newSchema: "users");

            migrationBuilder.CreateIndex(
                name: "ux_organizerprofile_user_id",
                schema: "users",
                table: "OrganizerProfile",
                column: "user_id",
                unique: true);
        }
    }
}
