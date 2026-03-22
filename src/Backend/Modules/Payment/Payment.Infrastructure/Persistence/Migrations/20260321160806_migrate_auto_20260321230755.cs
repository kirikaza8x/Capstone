using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class migrate_auto_20260321230755 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_wallet_transaction_status",
                schema: "payments",
                table: "wallet_transaction");

            migrationBuilder.DropIndex(
                name: "ix_payment_transaction_event_id",
                schema: "payments",
                table: "payment_transaction");

            migrationBuilder.DropIndex(
                name: "ix_payment_transaction_event_status",
                schema: "payments",
                table: "payment_transaction");

            migrationBuilder.DropIndex(
                name: "ix_payment_transaction_gateway_status",
                schema: "payments",
                table: "payment_transaction");

            migrationBuilder.DropIndex(
                name: "ix_payment_transaction_txn_ref",
                schema: "payments",
                table: "payment_transaction");

            migrationBuilder.DropColumn(
                name: "event_id",
                schema: "payments",
                table: "payment_transaction");

            migrationBuilder.AlterColumn<string>(
                name: "type",
                schema: "payments",
                table: "wallet_transaction",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "payments",
                table: "wallet_transaction",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "payments",
                table: "wallet_transaction",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "payments",
                table: "wallet",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Active",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<decimal>(
                name: "balance",
                schema: "payments",
                table: "wallet",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "payments",
                table: "wallet",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()");

            migrationBuilder.CreateTable(
                name: "batch_payment_item",
                schema: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    internal_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "AwaitingGateway"),
                    refunded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_batch_payment_item", x => x.id);
                    table.ForeignKey(
                        name: "fk_batch_payment_item_payment_transactions_payment_transaction",
                        column: x => x.payment_transaction_id,
                        principalSchema: "payments",
                        principalTable: "payment_transaction",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refund_request",
                schema: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_id = table.Column<Guid>(type: "uuid", nullable: true),
                    scope = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    requested_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    user_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    reviewer_note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    reviewed_by_admin_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refund_request", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_wallet_transaction_wallet_type",
                schema: "payments",
                table: "wallet_transaction",
                columns: new[] { "wallet_id", "type" });

            migrationBuilder.CreateIndex(
                name: "ix_payment_transaction_txn_ref",
                schema: "payments",
                table: "payment_transaction",
                column: "gateway_txn_ref");

            migrationBuilder.CreateIndex(
                name: "ix_payment_transaction_user_status",
                schema: "payments",
                table: "payment_transaction",
                columns: new[] { "user_id", "internal_status" });

            migrationBuilder.CreateIndex(
                name: "ix_batch_payment_item_event_id",
                schema: "payments",
                table: "batch_payment_item",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "ix_batch_payment_item_event_status",
                schema: "payments",
                table: "batch_payment_item",
                columns: new[] { "event_id", "internal_status" });

            migrationBuilder.CreateIndex(
                name: "ix_batch_payment_item_transaction_id",
                schema: "payments",
                table: "batch_payment_item",
                column: "payment_transaction_id");

            migrationBuilder.CreateIndex(
                name: "ix_refund_request_status",
                schema: "payments",
                table: "refund_request",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_refund_request_transaction_id",
                schema: "payments",
                table: "refund_request",
                column: "payment_transaction_id");

            migrationBuilder.CreateIndex(
                name: "ix_refund_request_txn_event_status",
                schema: "payments",
                table: "refund_request",
                columns: new[] { "payment_transaction_id", "event_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_refund_request_user_id",
                schema: "payments",
                table: "refund_request",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "batch_payment_item",
                schema: "payments");

            migrationBuilder.DropTable(
                name: "refund_request",
                schema: "payments");

            migrationBuilder.DropIndex(
                name: "ix_wallet_transaction_wallet_type",
                schema: "payments",
                table: "wallet_transaction");

            migrationBuilder.DropIndex(
                name: "ix_payment_transaction_txn_ref",
                schema: "payments",
                table: "payment_transaction");

            migrationBuilder.DropIndex(
                name: "ix_payment_transaction_user_status",
                schema: "payments",
                table: "payment_transaction");

            migrationBuilder.AlterColumn<string>(
                name: "type",
                schema: "payments",
                table: "wallet_transaction",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "payments",
                table: "wallet_transaction",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Pending");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "payments",
                table: "wallet_transaction",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "payments",
                table: "wallet",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Active");

            migrationBuilder.AlterColumn<decimal>(
                name: "balance",
                schema: "payments",
                table: "wallet",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "payments",
                table: "wallet",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "event_id",
                schema: "payments",
                table: "payment_transaction",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_wallet_transaction_status",
                schema: "payments",
                table: "wallet_transaction",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_payment_transaction_event_id",
                schema: "payments",
                table: "payment_transaction",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_transaction_event_status",
                schema: "payments",
                table: "payment_transaction",
                columns: new[] { "event_id", "internal_status" });

            migrationBuilder.CreateIndex(
                name: "ix_payment_transaction_gateway_status",
                schema: "payments",
                table: "payment_transaction",
                column: "gateway_status");

            migrationBuilder.CreateIndex(
                name: "ix_payment_transaction_txn_ref",
                schema: "payments",
                table: "payment_transaction",
                column: "gateway_txn_ref",
                unique: true);
        }
    }
}
