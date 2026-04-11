using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class migrate_auto_20260322223932 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_batch_payment_item_event_status",
                schema: "payments",
                table: "batch_payment_item");

            migrationBuilder.RenameColumn(
                name: "event_id",
                schema: "payments",
                table: "refund_request",
                newName: "event_session_id");

            migrationBuilder.RenameIndex(
                name: "ix_refund_request_txn_event_status",
                schema: "payments",
                table: "refund_request",
                newName: "ix_refund_request_txn_session_status");

            migrationBuilder.RenameColumn(
                name: "event_id",
                schema: "payments",
                table: "batch_payment_item",
                newName: "order_ticket_id");

            migrationBuilder.RenameIndex(
                name: "ix_batch_payment_item_event_id",
                schema: "payments",
                table: "batch_payment_item",
                newName: "ix_batch_payment_item_order_ticket_id");

            migrationBuilder.AddColumn<Guid>(
                name: "order_id",
                schema: "payments",
                table: "payment_transaction",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "event_session_id",
                schema: "payments",
                table: "batch_payment_item",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_payment_transaction_order_id",
                schema: "payments",
                table: "payment_transaction",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_batch_payment_item_session_id",
                schema: "payments",
                table: "batch_payment_item",
                column: "event_session_id");

            migrationBuilder.CreateIndex(
                name: "ix_batch_payment_item_session_status",
                schema: "payments",
                table: "batch_payment_item",
                columns: new[] { "event_session_id", "internal_status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_payment_transaction_order_id",
                schema: "payments",
                table: "payment_transaction");

            migrationBuilder.DropIndex(
                name: "ix_batch_payment_item_session_id",
                schema: "payments",
                table: "batch_payment_item");

            migrationBuilder.DropIndex(
                name: "ix_batch_payment_item_session_status",
                schema: "payments",
                table: "batch_payment_item");

            migrationBuilder.DropColumn(
                name: "order_id",
                schema: "payments",
                table: "payment_transaction");

            migrationBuilder.DropColumn(
                name: "event_session_id",
                schema: "payments",
                table: "batch_payment_item");

            migrationBuilder.RenameColumn(
                name: "event_session_id",
                schema: "payments",
                table: "refund_request",
                newName: "event_id");

            migrationBuilder.RenameIndex(
                name: "ix_refund_request_txn_session_status",
                schema: "payments",
                table: "refund_request",
                newName: "ix_refund_request_txn_event_status");

            migrationBuilder.RenameColumn(
                name: "order_ticket_id",
                schema: "payments",
                table: "batch_payment_item",
                newName: "event_id");

            migrationBuilder.RenameIndex(
                name: "ix_batch_payment_item_order_ticket_id",
                schema: "payments",
                table: "batch_payment_item",
                newName: "ix_batch_payment_item_event_id");

            migrationBuilder.CreateIndex(
                name: "ix_batch_payment_item_event_status",
                schema: "payments",
                table: "batch_payment_item",
                columns: new[] { "event_id", "internal_status" });
        }
    }
}
