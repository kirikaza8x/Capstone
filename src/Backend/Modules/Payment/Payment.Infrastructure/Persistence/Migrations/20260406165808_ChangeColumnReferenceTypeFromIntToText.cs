using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeColumnReferenceTypeFromIntToText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "reference_type",
                schema: "payments",
                table: "payment_transaction",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_payment_transaction_reference_id",
                schema: "payments",
                table: "payment_transaction",
                column: "reference_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_transaction_reference_type",
                schema: "payments",
                table: "payment_transaction",
                column: "reference_type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_payment_transaction_reference_id",
                schema: "payments",
                table: "payment_transaction");

            migrationBuilder.DropIndex(
                name: "ix_payment_transaction_reference_type",
                schema: "payments",
                table: "payment_transaction");

            migrationBuilder.AlterColumn<int>(
                name: "reference_type",
                schema: "payments",
                table: "payment_transaction",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);
        }
    }
}
