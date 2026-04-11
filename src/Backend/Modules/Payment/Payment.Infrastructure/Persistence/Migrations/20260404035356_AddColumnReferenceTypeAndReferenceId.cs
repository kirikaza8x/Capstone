using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnReferenceTypeAndReferenceId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "reference_id",
                schema: "payments",
                table: "payment_transaction",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "reference_type",
                schema: "payments",
                table: "payment_transaction",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "reference_id",
                schema: "payments",
                table: "payment_transaction");

            migrationBuilder.DropColumn(
                name: "reference_type",
                schema: "payments",
                table: "payment_transaction");
        }
    }
}
