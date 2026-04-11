using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Events.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnPublishRejectionReasonAndCancellationRejectionReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cancellation_rejection_reason",
                schema: "events",
                table: "events",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "publish_rejection_reason",
                schema: "events",
                table: "events",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cancellation_rejection_reason",
                schema: "events",
                table: "events");

            migrationBuilder.DropColumn(
                name: "publish_rejection_reason",
                schema: "events",
                table: "events");
        }
    }
}
