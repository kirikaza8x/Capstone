using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class update_payment_entities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "ix_payment_transaction_status",
                schema: "payments",
                table: "payment_transaction",
                newName: "ix_payment_transaction_gateway_status");

            migrationBuilder.AddColumn<DateTime>(
                name: "failed_at",
                schema: "payments",
                table: "payment_transaction",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "internal_status",
                schema: "payments",
                table: "payment_transaction",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "AwaitingGateway");

            migrationBuilder.AddColumn<DateTime>(
                name: "refunded_at",
                schema: "payments",
                table: "payment_transaction",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_payment_transaction_event_status",
                schema: "payments",
                table: "payment_transaction",
                columns: new[] { "event_id", "internal_status" });

            migrationBuilder.CreateIndex(
                name: "ix_payment_transaction_internal_status",
                schema: "payments",
                table: "payment_transaction",
                column: "internal_status");

            migrationBuilder.CreateIndex(
                name: "ix_payment_transaction_txn_ref",
                schema: "payments",
                table: "payment_transaction",
                column: "gateway_txn_ref",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_payment_transaction_event_status",
                schema: "payments",
                table: "payment_transaction");

            migrationBuilder.DropIndex(
                name: "ix_payment_transaction_internal_status",
                schema: "payments",
                table: "payment_transaction");

            migrationBuilder.DropIndex(
                name: "ix_payment_transaction_txn_ref",
                schema: "payments",
                table: "payment_transaction");

            migrationBuilder.DropColumn(
                name: "failed_at",
                schema: "payments",
                table: "payment_transaction");

            migrationBuilder.DropColumn(
                name: "internal_status",
                schema: "payments",
                table: "payment_transaction");

            migrationBuilder.DropColumn(
                name: "refunded_at",
                schema: "payments",
                table: "payment_transaction");

            migrationBuilder.RenameIndex(
                name: "ix_payment_transaction_gateway_status",
                schema: "payments",
                table: "payment_transaction",
                newName: "ix_payment_transaction_status");
        }
    }
}
