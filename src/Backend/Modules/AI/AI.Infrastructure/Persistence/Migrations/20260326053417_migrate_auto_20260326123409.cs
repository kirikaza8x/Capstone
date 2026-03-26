using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class migrate_auto_20260326123409 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_post_organizer_event_status",
                schema: "ai",
                table: "post");

            migrationBuilder.DropIndex(
                name: "ix_post_published_by_event",
                schema: "ai",
                table: "post");

            migrationBuilder.AlterColumn<string>(
                name: "prompt_used",
                schema: "ai",
                table: "post",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "body",
                schema: "ai",
                table: "post",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000);

            migrationBuilder.AddColumn<decimal>(
                name: "ai_cost",
                schema: "ai",
                table: "post",
                type: "numeric(10,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "slug",
                schema: "ai",
                table: "post",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "summary",
                schema: "ai",
                table: "post",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_post_event_published",
                schema: "ai",
                table: "post",
                columns: new[] { "event_id", "published_at" },
                filter: "status = 'Published'");

            migrationBuilder.CreateIndex(
                name: "ix_post_global_feed",
                schema: "ai",
                table: "post",
                columns: new[] { "status", "published_at" },
                filter: "status = 'Published'");

            migrationBuilder.CreateIndex(
                name: "ix_post_organizer_status_created",
                schema: "ai",
                table: "post",
                columns: new[] { "organizer_id", "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_post_slug",
                schema: "ai",
                table: "post",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_post_event_published",
                schema: "ai",
                table: "post");

            migrationBuilder.DropIndex(
                name: "ix_post_global_feed",
                schema: "ai",
                table: "post");

            migrationBuilder.DropIndex(
                name: "ix_post_organizer_status_created",
                schema: "ai",
                table: "post");

            migrationBuilder.DropIndex(
                name: "ix_post_slug",
                schema: "ai",
                table: "post");

            migrationBuilder.DropColumn(
                name: "ai_cost",
                schema: "ai",
                table: "post");

            migrationBuilder.DropColumn(
                name: "slug",
                schema: "ai",
                table: "post");

            migrationBuilder.DropColumn(
                name: "summary",
                schema: "ai",
                table: "post");

            migrationBuilder.AlterColumn<string>(
                name: "prompt_used",
                schema: "ai",
                table: "post",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "body",
                schema: "ai",
                table: "post",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "ix_post_organizer_event_status",
                schema: "ai",
                table: "post",
                columns: new[] { "organizer_id", "event_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_post_published_by_event",
                schema: "ai",
                table: "post",
                columns: new[] { "event_id", "status", "published_at" },
                filter: "status = 'Published'");
        }
    }
}
