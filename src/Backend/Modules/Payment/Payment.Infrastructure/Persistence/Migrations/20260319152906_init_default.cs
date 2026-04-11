using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class init_default : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "payments");

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    content = table.Column<string>(type: "jsonb", nullable: false),
                    occurred_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payment_transaction",
                schema: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_id = table.Column<Guid>(type: "uuid", nullable: true),
                    wallet_id = table.Column<Guid>(type: "uuid", nullable: true),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    gateway_transaction_no = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    gateway_response_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    gateway_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    gateway_order_info = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    gateway_txn_ref = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    gateway_bank_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    gateway_bank_tran_no = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    gateway_card_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    gateway_pay_date = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    gateway_tmn_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    gateway_secure_hash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    gateway_secure_hash_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    gateway_locale = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    gateway_ip_addr = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    gateway_create_date = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    gateway_order_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    gateway_merchant = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_transaction", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "wallet",
                schema: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wallet", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "wallet_transaction",
                schema: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    wallet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    direction = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    balance_before = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    balance_after = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wallet_transaction", x => x.id);
                    table.ForeignKey(
                        name: "fk_wallet_transaction_wallet_wallet_id",
                        column: x => x.wallet_id,
                        principalSchema: "payments",
                        principalTable: "wallet",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_processed_on_utc",
                schema: "payments",
                table: "outbox_messages",
                column: "processed_on_utc");

            migrationBuilder.CreateIndex(
                name: "ix_payment_transaction_event_id",
                schema: "payments",
                table: "payment_transaction",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_transaction_response_code",
                schema: "payments",
                table: "payment_transaction",
                column: "gateway_response_code");

            migrationBuilder.CreateIndex(
                name: "ix_payment_transaction_status",
                schema: "payments",
                table: "payment_transaction",
                column: "gateway_status");

            migrationBuilder.CreateIndex(
                name: "ix_payment_transaction_user_id",
                schema: "payments",
                table: "payment_transaction",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_transaction_wallet_id",
                schema: "payments",
                table: "payment_transaction",
                column: "wallet_id");

            migrationBuilder.CreateIndex(
                name: "ix_wallet_status",
                schema: "payments",
                table: "wallet",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_wallet_user_id",
                schema: "payments",
                table: "wallet",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_wallet_transaction_status",
                schema: "payments",
                table: "wallet_transaction",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_wallet_transaction_wallet_id",
                schema: "payments",
                table: "wallet_transaction",
                column: "wallet_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "payments");

            migrationBuilder.DropTable(
                name: "payment_transaction",
                schema: "payments");

            migrationBuilder.DropTable(
                name: "wallet_transaction",
                schema: "payments");

            migrationBuilder.DropTable(
                name: "wallet",
                schema: "payments");
        }
    }
}
