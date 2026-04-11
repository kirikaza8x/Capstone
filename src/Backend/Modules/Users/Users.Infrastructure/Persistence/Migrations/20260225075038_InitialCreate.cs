using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Users.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "users");

            migrationBuilder.CreateTable(
                name: "role",
                schema: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "NOW()"),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                schema: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    birthday = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    gender = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    social_link = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    profile_image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "NOW()"),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_session",
                schema: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    campaign_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_active_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_session", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "external_identity",
                schema: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    provider_key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    linked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "NOW()"),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_external_identity", x => x.id);
                    table.ForeignKey(
                        name: "fk_external_identity_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "users",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refresh_token",
                schema: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    token = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    expiry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_revoked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    device_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "NOW()"),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_token", x => x.id);
                    table.ForeignKey(
                        name: "fk_refresh_token_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "users",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                schema: "users",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_userroles_role",
                        column: x => x.role_id,
                        principalSchema: "users",
                        principalTable: "role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_userroles_user",
                        column: x => x.user_id,
                        principalSchema: "users",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wallet",
                schema: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.ForeignKey(
                        name: "fk_wallet_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "users",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wallet_transaction",
                schema: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    wallet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
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
                        principalSchema: "users",
                        principalTable: "wallet",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_external_identity_created_at",
                schema: "users",
                table: "external_identity",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_external_identity_provider_key",
                schema: "users",
                table: "external_identity",
                columns: new[] { "provider", "provider_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_external_identity_user_id",
                schema: "users",
                table: "external_identity",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_token_user_id",
                schema: "users",
                table: "refresh_token",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_created_at",
                schema: "users",
                table: "user",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_user_email",
                schema: "users",
                table: "user",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_is_active",
                schema: "users",
                table: "user",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_user_status",
                schema: "users",
                table: "user",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_user_username",
                schema: "users",
                table: "user",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_role_id",
                schema: "users",
                table: "user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_session_campaign_id",
                schema: "users",
                table: "user_session",
                column: "campaign_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_session_last_active_at",
                schema: "users",
                table: "user_session",
                column: "last_active_at");

            migrationBuilder.CreateIndex(
                name: "ix_user_session_session_id",
                schema: "users",
                table: "user_session",
                column: "session_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_session_user_id",
                schema: "users",
                table: "user_session",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_wallet_status",
                schema: "users",
                table: "wallet",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_wallet_user_id",
                schema: "users",
                table: "wallet",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_wallet_transaction_created_at",
                schema: "users",
                table: "wallet_transaction",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_wallet_transaction_status",
                schema: "users",
                table: "wallet_transaction",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_wallet_transaction_wallet_id",
                schema: "users",
                table: "wallet_transaction",
                column: "wallet_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "external_identity",
                schema: "users");

            migrationBuilder.DropTable(
                name: "refresh_token",
                schema: "users");

            migrationBuilder.DropTable(
                name: "user_roles",
                schema: "users");

            migrationBuilder.DropTable(
                name: "user_session",
                schema: "users");

            migrationBuilder.DropTable(
                name: "wallet_transaction",
                schema: "users");

            migrationBuilder.DropTable(
                name: "role",
                schema: "users");

            migrationBuilder.DropTable(
                name: "wallet",
                schema: "users");

            migrationBuilder.DropTable(
                name: "user",
                schema: "users");
        }
    }
}
