using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace AI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class init_migrate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ai");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "event_embeddings",
                schema: "ai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    embedding = table.Column<Vector>(type: "vector(384)", nullable: false),
                    model_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    embedded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "NOW()"),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_embeddings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "event_snapshots",
                schema: "ai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "varchar(500)", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    snapshot_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    categories = table.Column<string>(type: "jsonb", nullable: false),
                    hashtags = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "NOW()"),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_snapshots", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "global_category_stat",
                schema: "ai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    popularity_score = table.Column<double>(type: "double precision", nullable: false),
                    total_interactions = table.Column<int>(type: "integer", nullable: false),
                    last_calculated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    first_seen = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    raw_weighted_score = table.Column<double>(type: "double precision", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "NOW()"),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_global_category_stat", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "interaction_weight",
                schema: "ai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    action_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    weight = table.Column<double>(type: "double precision", nullable: false),
                    description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "default"),
                    deactivated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "NOW()"),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_interaction_weight", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "ai",
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
                name: "user_behavior_log",
                schema: "ai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    target_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    target_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    session_id = table.Column<string>(type: "text", nullable: true),
                    device_type = table.Column<string>(type: "text", nullable: true),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    metadata = table.Column<Dictionary<string, string>>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "NOW()"),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_behavior_log", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_embedding",
                schema: "ai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    embedding = table.Column<Vector>(type: "vector(384)", nullable: false),
                    dimension = table.Column<int>(type: "integer", nullable: false),
                    interaction_count = table.Column<int>(type: "integer", nullable: false),
                    confidence = table.Column<double>(type: "double precision", nullable: false),
                    last_calculated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_stale = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    contributing_categories = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "NOW()"),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_embedding", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_interest_score",
                schema: "ai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    score = table.Column<double>(type: "double precision", nullable: false),
                    total_interactions = table.Column<int>(type: "integer", nullable: false),
                    last_updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "NOW()"),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_interest_score", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_event_embeddings_embedding",
                schema: "ai",
                table: "event_embeddings",
                column: "embedding")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" })
                .Annotation("Npgsql:StorageParameter:ef_construction", 64)
                .Annotation("Npgsql:StorageParameter:m", 16);

            migrationBuilder.CreateIndex(
                name: "ix_event_embeddings_event_id",
                schema: "ai",
                table: "event_embeddings",
                column: "event_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_event_snapshots_active_updated",
                schema: "ai",
                table: "event_snapshots",
                columns: new[] { "is_active", "snapshot_updated_at" });

            migrationBuilder.CreateIndex(
                name: "ix_event_snapshots_categories_gin",
                schema: "ai",
                table: "event_snapshots",
                column: "categories")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "jsonb_path_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_event_snapshots_hashtags_gin",
                schema: "ai",
                table: "event_snapshots",
                column: "hashtags")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "jsonb_path_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_event_snapshots_is_active",
                schema: "ai",
                table: "event_snapshots",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_event_snapshots_title",
                schema: "ai",
                table: "event_snapshots",
                column: "title");

            migrationBuilder.CreateIndex(
                name: "ix_event_snapshots_updated_at",
                schema: "ai",
                table: "event_snapshots",
                column: "snapshot_updated_at");

            migrationBuilder.CreateIndex(
                name: "ux_global_category_stat_category",
                schema: "ai",
                table: "global_category_stat",
                column: "category",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_interaction_weight_action_version_active",
                schema: "ai",
                table: "interaction_weight",
                columns: new[] { "action_type", "version", "is_active" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_processed_on_utc",
                schema: "ai",
                table: "outbox_messages",
                column: "processed_on_utc");

            migrationBuilder.CreateIndex(
                name: "ix_user_behavior_log_occurred",
                schema: "ai",
                table: "user_behavior_log",
                column: "occurred_at");

            migrationBuilder.CreateIndex(
                name: "ix_user_behavior_log_user",
                schema: "ai",
                table: "user_behavior_log",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_behavior_log_user_action",
                schema: "ai",
                table: "user_behavior_log",
                columns: new[] { "user_id", "action_type" });

            migrationBuilder.CreateIndex(
                name: "ix_user_embedding_embedding",
                schema: "ai",
                table: "user_embedding",
                column: "embedding")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" })
                .Annotation("Npgsql:StorageParameter:ef_construction", 64)
                .Annotation("Npgsql:StorageParameter:m", 16);

            migrationBuilder.CreateIndex(
                name: "ix_user_embedding_is_stale",
                schema: "ai",
                table: "user_embedding",
                column: "is_stale");

            migrationBuilder.CreateIndex(
                name: "ix_user_embedding_user_id",
                schema: "ai",
                table: "user_embedding",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ux_user_interest_score_user_category",
                schema: "ai",
                table: "user_interest_score",
                columns: new[] { "user_id", "category" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "event_embeddings",
                schema: "ai");

            migrationBuilder.DropTable(
                name: "event_snapshots",
                schema: "ai");

            migrationBuilder.DropTable(
                name: "global_category_stat",
                schema: "ai");

            migrationBuilder.DropTable(
                name: "interaction_weight",
                schema: "ai");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "ai");

            migrationBuilder.DropTable(
                name: "user_behavior_log",
                schema: "ai");

            migrationBuilder.DropTable(
                name: "user_embedding",
                schema: "ai");

            migrationBuilder.DropTable(
                name: "user_interest_score",
                schema: "ai");
        }
    }
}
