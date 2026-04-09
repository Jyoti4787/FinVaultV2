using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinVault.CardService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompositeIndex_UserId_IsDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CreditCards_UserId",
                table: "CreditCards",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CreditCards_UserId",
                table: "CreditCards");
        }
    }
}
