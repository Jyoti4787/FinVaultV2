using System;
using Microsoft.EntityFrameworkCore.Migrations;
using FinVault.PaymentService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace FinVault.PaymentService.Infrastructure.Migrations
{
    [DbContext(typeof(PaymentDbContext))]
    [Migration("20260409120000_AddCardIdToRewardPoints")]
    /// <inheritdoc />
    public partial class AddCardIdToRewardPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CardId",
                table: "RewardPoints",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RewardPoints_CardId",
                table: "RewardPoints",
                column: "CardId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RewardPoints_CardId",
                table: "RewardPoints");

            migrationBuilder.DropColumn(
                name: "CardId",
                table: "RewardPoints");
        }
    }
}
