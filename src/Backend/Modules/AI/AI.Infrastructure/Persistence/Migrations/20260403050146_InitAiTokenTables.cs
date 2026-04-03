using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitAiTokenTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ai_package",
                schema: "ai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    token_quota = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "NOW()"),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ai_package", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "organizer_ai_quota",
                schema: "ai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organizer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subscription_tokens = table.Column<int>(type: "integer", nullable: false),
                    top_up_tokens = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "NOW()"),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organizer_ai_quota", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ai_token_transaction",
                schema: "ai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    quota_id = table.Column<Guid>(type: "uuid", nullable: false),
                    package_id = table.Column<Guid>(type: "uuid", nullable: true),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    amount = table.Column<int>(type: "integer", nullable: false),
                    balance_after = table.Column<int>(type: "integer", nullable: false),
                    reference_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "NOW()"),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ai_token_transaction", x => x.id);
                    table.ForeignKey(
                        name: "fk_ai_token_transaction_ai_package_package_id",
                        column: x => x.package_id,
                        principalSchema: "ai",
                        principalTable: "ai_package",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_ai_token_transaction_organizer_ai_quotas_quota_id",
                        column: x => x.quota_id,
                        principalSchema: "ai",
                        principalTable: "organizer_ai_quota",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ai_package_name",
                schema: "ai",
                table: "ai_package",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_ai_package_type",
                schema: "ai",
                table: "ai_package",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "ix_ai_token_transaction_created_at",
                schema: "ai",
                table: "ai_token_transaction",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_ai_token_transaction_package_id",
                schema: "ai",
                table: "ai_token_transaction",
                column: "package_id");

            migrationBuilder.CreateIndex(
                name: "ix_ai_token_transaction_quota_id",
                schema: "ai",
                table: "ai_token_transaction",
                column: "quota_id");

            migrationBuilder.CreateIndex(
                name: "ix_ai_token_transaction_type",
                schema: "ai",
                table: "ai_token_transaction",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "ux_organizer_ai_quota_organizer",
                schema: "ai",
                table: "organizer_ai_quota",
                column: "organizer_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_token_transaction",
                schema: "ai");

            migrationBuilder.DropTable(
                name: "ai_package",
                schema: "ai");

            migrationBuilder.DropTable(
                name: "organizer_ai_quota",
                schema: "ai");
        }
    }
}
