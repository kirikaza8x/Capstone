using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Events.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameEvenStaffToEventMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "event_staffs",
                schema: "events");

            migrationBuilder.CreateTable(
                name: "event_members",
                schema: "events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permissions = table.Column<List<string>>(type: "text[]", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    assigned_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_members", x => x.id);
                    table.ForeignKey(
                        name: "fk_event_members_events_event_id",
                        column: x => x.event_id,
                        principalSchema: "events",
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_event_members_event_id",
                schema: "events",
                table: "event_members",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_members_event_id_user_id",
                schema: "events",
                table: "event_members",
                columns: new[] { "event_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_event_members_user_id",
                schema: "events",
                table: "event_members",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "event_members",
                schema: "events");

            migrationBuilder.CreateTable(
                name: "event_staffs",
                schema: "events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    permissions = table.Column<List<string>>(type: "text[]", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_staffs", x => x.id);
                    table.ForeignKey(
                        name: "fk_event_staffs_events_event_id",
                        column: x => x.event_id,
                        principalSchema: "events",
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_event_staffs_event_id",
                schema: "events",
                table: "event_staffs",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_staffs_event_id_user_id",
                schema: "events",
                table: "event_staffs",
                columns: new[] { "event_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_event_staffs_user_id",
                schema: "events",
                table: "event_staffs",
                column: "user_id");
        }
    }
}
