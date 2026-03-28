using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticketing.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnNameAndDescForVoucher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "description",
                schema: "ticketing",
                table: "vouchers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "name",
                schema: "ticketing",
                table: "vouchers",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description",
                schema: "ticketing",
                table: "vouchers");

            migrationBuilder.DropColumn(
                name: "name",
                schema: "ticketing",
                table: "vouchers");
        }
    }
}
