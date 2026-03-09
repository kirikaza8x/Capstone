using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Users.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class add_organizer_entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrganizerProfile",
                schema: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    logo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    display_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    social_link = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    business_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    tax_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    identity_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    company_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    account_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    account_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    bank_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    branch = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Draft"),
                    verified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "NOW()"),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organizer_profile", x => x.id);
                    table.ForeignKey(
                        name: "fk_organizer_profile_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "users",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_organizerprofile_business_type",
                schema: "users",
                table: "OrganizerProfile",
                column: "business_type");

            migrationBuilder.CreateIndex(
                name: "ix_organizerprofile_created_at",
                schema: "users",
                table: "OrganizerProfile",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_organizerprofile_display_name",
                schema: "users",
                table: "OrganizerProfile",
                column: "display_name");

            migrationBuilder.CreateIndex(
                name: "ix_organizerprofile_status",
                schema: "users",
                table: "OrganizerProfile",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ux_organizerprofile_user_id",
                schema: "users",
                table: "OrganizerProfile",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganizerProfile",
                schema: "users");
        }
    }
}
