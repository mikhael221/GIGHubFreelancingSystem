using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Freelancing.Migrations
{
    /// <inheritdoc />
    public partial class UpdateManageBiddings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Biddings_UserAccounts_UserAccountId",
                table: "Biddings");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_UserAccounts_UserAccountId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_UserAccountId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Biddings_UserAccountId",
                table: "Biddings");

            migrationBuilder.DropColumn(
                name: "UserAccountId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "UserAccountId",
                table: "Biddings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserAccountId",
                table: "Projects",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserAccountId",
                table: "Biddings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_UserAccountId",
                table: "Projects",
                column: "UserAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Biddings_UserAccountId",
                table: "Biddings",
                column: "UserAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Biddings_UserAccounts_UserAccountId",
                table: "Biddings",
                column: "UserAccountId",
                principalTable: "UserAccounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_UserAccounts_UserAccountId",
                table: "Projects",
                column: "UserAccountId",
                principalTable: "UserAccounts",
                principalColumn: "Id");
        }
    }
}
