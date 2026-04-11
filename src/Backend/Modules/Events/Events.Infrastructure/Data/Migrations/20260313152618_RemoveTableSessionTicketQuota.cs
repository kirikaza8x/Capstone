using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Events.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTableSessionTicketQuota : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "session_ticket_quotas",
                schema: "events");

            migrationBuilder.AddColumn<int>(
                name: "quantity",
                schema: "events",
                table: "ticket_types",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "quantity",
                schema: "events",
                table: "ticket_types");

            migrationBuilder.CreateTable(
                name: "session_ticket_quotas",
                schema: "events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ticket_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_session_ticket_quotas", x => x.id);
                    table.ForeignKey(
                        name: "fk_session_ticket_quotas_event_sessions_event_session_id",
                        column: x => x.event_session_id,
                        principalSchema: "events",
                        principalTable: "event_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_session_ticket_quotas_ticket_types_ticket_type_id",
                        column: x => x.ticket_type_id,
                        principalSchema: "events",
                        principalTable: "ticket_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_session_ticket_quota_session_tickettype",
                schema: "events",
                table: "session_ticket_quotas",
                columns: new[] { "event_session_id", "ticket_type_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_session_ticket_quotas_ticket_type_id",
                schema: "events",
                table: "session_ticket_quotas",
                column: "ticket_type_id");
        }
    }
}
