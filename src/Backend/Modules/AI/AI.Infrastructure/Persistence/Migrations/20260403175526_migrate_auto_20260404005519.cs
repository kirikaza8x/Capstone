using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class migrate_auto_20260404005519 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "social_post_analytics",
                schema: "ai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_marketing_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_post_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    platform = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    impressions = table.Column<long>(type: "bigint", nullable: false),
                    clicks = table.Column<long>(type: "bigint", nullable: false),
                    reactions = table.Column<long>(type: "bigint", nullable: false),
                    shares = table.Column<long>(type: "bigint", nullable: false),
                    video_views = table.Column<long>(type: "bigint", nullable: false),
                    reach = table.Column<long>(type: "bigint", nullable: false),
                    fetched_date = table.Column<DateOnly>(type: "date", nullable: false),
                    fetched_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_social_post_analytics", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_social_analytics_post_day",
                schema: "ai",
                table: "social_post_analytics",
                columns: new[] { "post_marketing_id", "fetched_date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "social_post_analytics",
                schema: "ai");
        }
    }
}
