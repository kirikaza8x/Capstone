using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Events.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameEventCategoryMappingAndEventCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "event_category_mappings",
                schema: "events");

            migrationBuilder.DropPrimaryKey(
                name: "pk_event_categories",
                schema: "events",
                table: "event_categories");

            migrationBuilder.DropIndex(
                name: "ix_event_categories_code",
                schema: "events",
                table: "event_categories");

            migrationBuilder.DropColumn(
                name: "code",
                schema: "events",
                table: "event_categories");

            migrationBuilder.DropColumn(
                name: "created_at",
                schema: "events",
                table: "event_categories");

            migrationBuilder.DropColumn(
                name: "created_by",
                schema: "events",
                table: "event_categories");

            migrationBuilder.DropColumn(
                name: "description",
                schema: "events",
                table: "event_categories");

            migrationBuilder.DropColumn(
                name: "is_active",
                schema: "events",
                table: "event_categories");

            migrationBuilder.DropColumn(
                name: "modified_at",
                schema: "events",
                table: "event_categories");

            migrationBuilder.DropColumn(
                name: "modified_by",
                schema: "events",
                table: "event_categories");

            migrationBuilder.DropColumn(
                name: "name",
                schema: "events",
                table: "event_categories");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "events",
                table: "event_categories",
                newName: "category_id");

            migrationBuilder.AlterColumn<int>(
                name: "category_id",
                schema: "events",
                table: "event_categories",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<Guid>(
                name: "event_id",
                schema: "events",
                table: "event_categories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "pk_event_categories",
                schema: "events",
                table: "event_categories",
                columns: new[] { "event_id", "category_id" });

            migrationBuilder.CreateTable(
                name: "categories",
                schema: "events",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_event_categories_category_id",
                schema: "events",
                table: "event_categories",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_categories_code",
                schema: "events",
                table: "categories",
                column: "code");

            migrationBuilder.AddForeignKey(
                name: "fk_event_categories_categories_category_id",
                schema: "events",
                table: "event_categories",
                column: "category_id",
                principalSchema: "events",
                principalTable: "categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_event_categories_events_event_id",
                schema: "events",
                table: "event_categories",
                column: "event_id",
                principalSchema: "events",
                principalTable: "events",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_event_categories_categories_category_id",
                schema: "events",
                table: "event_categories");

            migrationBuilder.DropForeignKey(
                name: "fk_event_categories_events_event_id",
                schema: "events",
                table: "event_categories");

            migrationBuilder.DropTable(
                name: "categories",
                schema: "events");

            migrationBuilder.DropPrimaryKey(
                name: "pk_event_categories",
                schema: "events",
                table: "event_categories");

            migrationBuilder.DropIndex(
                name: "ix_event_categories_category_id",
                schema: "events",
                table: "event_categories");

            migrationBuilder.DropColumn(
                name: "event_id",
                schema: "events",
                table: "event_categories");

            migrationBuilder.RenameColumn(
                name: "category_id",
                schema: "events",
                table: "event_categories",
                newName: "id");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                schema: "events",
                table: "event_categories",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "code",
                schema: "events",
                table: "event_categories",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                schema: "events",
                table: "event_categories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                schema: "events",
                table: "event_categories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description",
                schema: "events",
                table: "event_categories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                schema: "events",
                table: "event_categories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "modified_at",
                schema: "events",
                table: "event_categories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "modified_by",
                schema: "events",
                table: "event_categories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "name",
                schema: "events",
                table: "event_categories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "pk_event_categories",
                schema: "events",
                table: "event_categories",
                column: "id");

            migrationBuilder.CreateTable(
                name: "event_category_mappings",
                schema: "events",
                columns: table => new
                {
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_category_mappings", x => new { x.event_id, x.category_id });
                    table.ForeignKey(
                        name: "fk_event_category_mappings_event_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "events",
                        principalTable: "event_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_event_category_mappings_events_event_id",
                        column: x => x.event_id,
                        principalSchema: "events",
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_event_categories_code",
                schema: "events",
                table: "event_categories",
                column: "code");

            migrationBuilder.CreateIndex(
                name: "ix_event_category_mappings_category_id",
                schema: "events",
                table: "event_category_mappings",
                column: "category_id");
        }
    }
}
