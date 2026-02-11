using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Events.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateEventSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "events");

            migrationBuilder.CreateTable(
                name: "event_categories",
                schema: "events",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "events",
                schema: "events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organizer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    banner_url = table.Column<string>(type: "text", nullable: true),
                    location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    map_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    description = table.Column<string>(type: "text", nullable: false),
                    url_path = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    event_category_id = table.Column<int>(type: "integer", nullable: false),
                    ticket_sale_start_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ticket_sale_end_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    event_start_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    event_end_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    policy = table.Column<string>(type: "text", nullable: false),
                    spec = table.Column<string>(type: "jsonb", nullable: true),
                    seatmap_image = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    event_type_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "hashtags",
                schema: "events",
                columns: table => new
                {
                    hashtag_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    usage_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hashtags", x => x.hashtag_id);
                });

            migrationBuilder.CreateTable(
                name: "areas",
                schema: "events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    capacity = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_areas", x => x.id);
                    table.ForeignKey(
                        name: "fk_areas_events_event_id",
                        column: x => x.event_id,
                        principalSchema: "events",
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "event_actor_images",
                schema: "events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    major = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    image = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_actor_images", x => x.id);
                    table.ForeignKey(
                        name: "fk_event_actor_images_events_event_id",
                        column: x => x.event_id,
                        principalSchema: "events",
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "event_category_mappings",
                schema: "events",
                columns: table => new
                {
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_category_mappings", x => new { x.event_id, x.category_id });
                    table.ForeignKey(
                        name: "fk_event_category_mappings_event_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "events",
                        principalTable: "event_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_event_category_mappings_events_event_id",
                        column: x => x.event_id,
                        principalSchema: "events",
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "event_images",
                schema: "events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    image_url = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_images", x => x.id);
                    table.ForeignKey(
                        name: "fk_event_images_events_event_id",
                        column: x => x.event_id,
                        principalSchema: "events",
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "event_sessions",
                schema: "events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_sessions", x => x.id);
                    table.ForeignKey(
                        name: "fk_event_sessions_events_event_id",
                        column: x => x.event_id,
                        principalSchema: "events",
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "event_staffs",
                schema: "events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permissions = table.Column<List<string>>(type: "text[]", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    assigned_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_staffs", x => x.id);
                    table.ForeignKey(
                        name: "fk_event_staffs_events_event_id",
                        column: x => x.event_id,
                        principalSchema: "events",
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "event_hashtags",
                schema: "events",
                columns: table => new
                {
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    hashtag_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_hashtags", x => new { x.event_id, x.hashtag_id });
                    table.ForeignKey(
                        name: "fk_event_hashtags_events_event_id",
                        column: x => x.event_id,
                        principalSchema: "events",
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_event_hashtags_hashtags_hashtag_id",
                        column: x => x.hashtag_id,
                        principalSchema: "events",
                        principalTable: "hashtags",
                        principalColumn: "hashtag_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "seats",
                schema: "events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    area_id = table.Column<Guid>(type: "uuid", nullable: false),
                    seat_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    row_label = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    column_label = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    x = table.Column<float>(type: "real", nullable: false),
                    y = table.Column<float>(type: "real", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_seats", x => x.id);
                    table.ForeignKey(
                        name: "fk_seats_areas_area_id",
                        column: x => x.area_id,
                        principalSchema: "events",
                        principalTable: "areas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ticket_types",
                schema: "events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    sold_quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    area_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ticket_types", x => x.id);
                    table.ForeignKey(
                        name: "fk_ticket_types_areas_area_id",
                        column: x => x.area_id,
                        principalSchema: "events",
                        principalTable: "areas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_ticket_types_event_sessions_event_session_id",
                        column: x => x.event_session_id,
                        principalSchema: "events",
                        principalTable: "event_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "session_seat_statuses",
                schema: "events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    seat_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_session_seat_statuses", x => x.id);
                    table.ForeignKey(
                        name: "fk_session_seat_statuses_event_sessions_event_session_id",
                        column: x => x.event_session_id,
                        principalSchema: "events",
                        principalTable: "event_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_session_seat_statuses_seats_seat_id",
                        column: x => x.seat_id,
                        principalSchema: "events",
                        principalTable: "seats",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_areas_event_id",
                schema: "events",
                table: "areas",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_actor_images_event_id",
                schema: "events",
                table: "event_actor_images",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_categories_code",
                schema: "events",
                table: "event_categories",
                column: "code");

            migrationBuilder.CreateIndex(
                name: "ix_event_category_mappings_category_id",
                schema: "events",
                table: "event_category_mappings",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_hashtags_hashtag_id",
                schema: "events",
                table: "event_hashtags",
                column: "hashtag_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_images_event_id",
                schema: "events",
                table: "event_images",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_sessions_event_id",
                schema: "events",
                table: "event_sessions",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_staffs_event_id",
                schema: "events",
                table: "event_staffs",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_staffs_event_id_user_id",
                schema: "events",
                table: "event_staffs",
                columns: new[] { "event_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_event_staffs_user_id",
                schema: "events",
                table: "event_staffs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_events_organizer_id",
                schema: "events",
                table: "events",
                column: "organizer_id");

            migrationBuilder.CreateIndex(
                name: "ix_events_status",
                schema: "events",
                table: "events",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_events_url_path",
                schema: "events",
                table: "events",
                column: "url_path",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_hashtags_name",
                schema: "events",
                table: "hashtags",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_hashtags_slug",
                schema: "events",
                table: "hashtags",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_seats_area_id",
                schema: "events",
                table: "seats",
                column: "area_id");

            migrationBuilder.CreateIndex(
                name: "ix_seats_area_id_seat_code",
                schema: "events",
                table: "seats",
                columns: new[] { "area_id", "seat_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_session_seat_statuses_event_session_id",
                schema: "events",
                table: "session_seat_statuses",
                column: "event_session_id");

            migrationBuilder.CreateIndex(
                name: "ix_session_seat_statuses_event_session_id_seat_id",
                schema: "events",
                table: "session_seat_statuses",
                columns: new[] { "event_session_id", "seat_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_session_seat_statuses_seat_id",
                schema: "events",
                table: "session_seat_statuses",
                column: "seat_id");

            migrationBuilder.CreateIndex(
                name: "ix_ticket_types_area_id",
                schema: "events",
                table: "ticket_types",
                column: "area_id");

            migrationBuilder.CreateIndex(
                name: "ix_ticket_types_event_session_id",
                schema: "events",
                table: "ticket_types",
                column: "event_session_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "event_actor_images",
                schema: "events");

            migrationBuilder.DropTable(
                name: "event_category_mappings",
                schema: "events");

            migrationBuilder.DropTable(
                name: "event_hashtags",
                schema: "events");

            migrationBuilder.DropTable(
                name: "event_images",
                schema: "events");

            migrationBuilder.DropTable(
                name: "event_staffs",
                schema: "events");

            migrationBuilder.DropTable(
                name: "session_seat_statuses",
                schema: "events");

            migrationBuilder.DropTable(
                name: "ticket_types",
                schema: "events");

            migrationBuilder.DropTable(
                name: "event_categories",
                schema: "events");

            migrationBuilder.DropTable(
                name: "hashtags",
                schema: "events");

            migrationBuilder.DropTable(
                name: "seats",
                schema: "events");

            migrationBuilder.DropTable(
                name: "event_sessions",
                schema: "events");

            migrationBuilder.DropTable(
                name: "areas",
                schema: "events");

            migrationBuilder.DropTable(
                name: "events",
                schema: "events");
        }
    }
}
