using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class config_update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "version",
                table: "interaction_weight",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "default",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "ux_user_interest_score_user_category",
                table: "user_interest_score",
                columns: new[] { "user_id", "category" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_behavior_log_occurred",
                table: "user_behavior_log",
                column: "occurred_at");

            migrationBuilder.CreateIndex(
                name: "ix_user_behavior_log_user",
                table: "user_behavior_log",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_behavior_log_user_action",
                table: "user_behavior_log",
                columns: new[] { "user_id", "action_type" });

            migrationBuilder.CreateIndex(
                name: "ux_interaction_weight_action_version_active",
                table: "interaction_weight",
                columns: new[] { "action_type", "version", "is_active" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_global_category_stat_category",
                table: "global_category_stat",
                column: "category",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_user_interest_score_user_category",
                table: "user_interest_score");

            migrationBuilder.DropIndex(
                name: "ix_user_behavior_log_occurred",
                table: "user_behavior_log");

            migrationBuilder.DropIndex(
                name: "ix_user_behavior_log_user",
                table: "user_behavior_log");

            migrationBuilder.DropIndex(
                name: "ix_user_behavior_log_user_action",
                table: "user_behavior_log");

            migrationBuilder.DropIndex(
                name: "ux_interaction_weight_action_version_active",
                table: "interaction_weight");

            migrationBuilder.DropIndex(
                name: "ux_global_category_stat_category",
                table: "global_category_stat");

            migrationBuilder.AlterColumn<string>(
                name: "version",
                table: "interaction_weight",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "default");
        }
    }
}
