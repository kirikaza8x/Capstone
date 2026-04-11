using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class migrate_auto_20260403202210 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "external_post_url",
                schema: "ai",
                table: "post");

            migrationBuilder.CreateTable(
                name: "external_distribution",
                schema: "ai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    platform = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    external_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    external_post_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    platform_metadata = table.Column<string>(type: "jsonb", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error_message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    post_marketing_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_external_distribution", x => x.id);
                    table.ForeignKey(
                        name: "fk_external_distribution_post_marketing_post_marketing_id",
                        column: x => x.post_marketing_id,
                        principalSchema: "ai",
                        principalTable: "post",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_external_dist_platform_status",
                schema: "ai",
                table: "external_distribution",
                columns: new[] { "platform", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_external_dist_post_id",
                schema: "ai",
                table: "external_distribution",
                column: "post_marketing_id");

            migrationBuilder.CreateIndex(
                name: "ix_external_dist_status",
                schema: "ai",
                table: "external_distribution",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "external_distribution",
                schema: "ai");

            migrationBuilder.AddColumn<string>(
                name: "external_post_url",
                schema: "ai",
                table: "post",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
