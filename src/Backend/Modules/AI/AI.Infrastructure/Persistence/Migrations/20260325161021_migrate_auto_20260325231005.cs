using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class migrate_auto_20260325231005 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "post",
                schema: "ai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    organizer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    body = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    prompt_used = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ai_model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ai_tokens_used = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Draft"),
                    reviewed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    rejection_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    tracking_token = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    external_post_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_post", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_post_organizer_event_status",
                schema: "ai",
                table: "post",
                columns: new[] { "organizer_id", "event_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_post_pending_queue",
                schema: "ai",
                table: "post",
                columns: new[] { "status", "submitted_at" },
                filter: "status = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "ix_post_published_by_event",
                schema: "ai",
                table: "post",
                columns: new[] { "event_id", "status", "published_at" },
                filter: "status = 'Published'");

            migrationBuilder.CreateIndex(
                name: "ix_post_tracking_token",
                schema: "ai",
                table: "post",
                column: "tracking_token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "post",
                schema: "ai");
        }
    }
}
