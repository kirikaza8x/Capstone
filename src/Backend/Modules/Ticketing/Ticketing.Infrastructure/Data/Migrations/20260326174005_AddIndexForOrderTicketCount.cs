using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticketing.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexForOrderTicketCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_OrderTicket_Session_TicketType_Status",
                schema: "ticketing",
                table: "order_tickets",
                columns: new[] { "event_session_id", "ticket_type_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderTicket_Session_TicketType_Status",
                schema: "ticketing",
                table: "order_tickets");
        }
    }
}
