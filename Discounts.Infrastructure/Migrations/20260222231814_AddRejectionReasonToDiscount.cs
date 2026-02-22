using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Discounts.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRejectionReasonToDiscount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Discounts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Discounts");
        }
    }
}
