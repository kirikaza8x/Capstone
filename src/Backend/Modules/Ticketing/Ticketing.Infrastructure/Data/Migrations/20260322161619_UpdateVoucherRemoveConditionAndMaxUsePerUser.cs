using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticketing.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVoucherRemoveConditionAndMaxUsePerUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "condition",
                schema: "ticketing",
                table: "vouchers");

            migrationBuilder.DropColumn(
                name: "max_use_per_user",
                schema: "ticketing",
                table: "vouchers");

            migrationBuilder.AddColumn<int>(
                name: "max_use",
                schema: "ticketing",
                table: "vouchers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "max_use",
                schema: "ticketing",
                table: "vouchers");

            migrationBuilder.AddColumn<string>(
                name: "condition",
                schema: "ticketing",
                table: "vouchers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<short>(
                name: "max_use_per_user",
                schema: "ticketing",
                table: "vouchers",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);
        }
    }
}
