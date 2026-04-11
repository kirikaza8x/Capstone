using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Events.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSessionSeatStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "session_seat_statuses",
                schema: "events");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "session_seat_statuses",
                schema: "events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    seat_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_session_seat_statuses", x => x.id);
                    table.ForeignKey(
                        name: "fk_session_seat_statuses_event_sessions_event_session_id",
                        column: x => x.event_session_id,
                        principalSchema: "events",
                        principalTable: "event_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_session_seat_statuses_seats_seat_id",
                        column: x => x.seat_id,
                        principalSchema: "events",
                        principalTable: "seats",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_session_seat_statuses_event_session_id",
                schema: "events",
                table: "session_seat_statuses",
                column: "event_session_id");

            migrationBuilder.CreateIndex(
                name: "ix_session_seat_statuses_event_session_id_seat_id",
                schema: "events",
                table: "session_seat_statuses",
                columns: new[] { "event_session_id", "seat_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_session_seat_statuses_seat_id",
                schema: "events",
                table: "session_seat_statuses",
                column: "seat_id");
        }
    }
}
