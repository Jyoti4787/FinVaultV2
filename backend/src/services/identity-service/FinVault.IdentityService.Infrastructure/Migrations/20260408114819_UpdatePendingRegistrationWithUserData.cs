using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinVault.IdentityService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePendingRegistrationWithUserData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "PendingRegistrations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "PendingRegistrations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "PendingRegistrations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "PendingRegistrations");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "PendingRegistrations");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "PendingRegistrations");
        }
    }
}
