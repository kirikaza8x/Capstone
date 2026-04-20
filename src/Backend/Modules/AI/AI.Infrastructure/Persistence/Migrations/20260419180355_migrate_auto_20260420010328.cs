using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class migrate_auto_20260420010328 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_external_distribution_post_marketing_post_marketing_id",
                schema: "ai",
                table: "external_distribution");

            migrationBuilder.AddForeignKey(
                name: "fk_external_distribution_posts_post_marketing_id",
                schema: "ai",
                table: "external_distribution",
                column: "post_marketing_id",
                principalSchema: "ai",
                principalTable: "post",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_external_distribution_posts_post_marketing_id",
                schema: "ai",
                table: "external_distribution");

            migrationBuilder.AddForeignKey(
                name: "fk_external_distribution_post_marketing_post_marketing_id",
                schema: "ai",
                table: "external_distribution",
                column: "post_marketing_id",
                principalSchema: "ai",
                principalTable: "post",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
