using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class migrate_auto_20260326123708 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "ix_post_slug",
                schema: "ai",
                table: "post",
                newName: "ix_post_marketing_slug");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "ix_post_marketing_slug",
                schema: "ai",
                table: "post",
                newName: "ix_post_slug");
        }
    }
}
