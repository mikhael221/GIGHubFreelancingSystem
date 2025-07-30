using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Freelancing.Migrations
{
    /// <inheritdoc />
    public partial class mentorshiprelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MentorshipId",
                table: "UserAccounts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_MentorshipId",
                table: "UserAccounts",
                column: "MentorshipId",
                unique: true,
                filter: "[MentorshipId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAccounts_PeerMentorships_MentorshipId",
                table: "UserAccounts",
                column: "MentorshipId",
                principalTable: "PeerMentorships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAccounts_PeerMentorships_MentorshipId",
                table: "UserAccounts");

            migrationBuilder.DropIndex(
                name: "IX_UserAccounts_MentorshipId",
                table: "UserAccounts");

            migrationBuilder.DropColumn(
                name: "MentorshipId",
                table: "UserAccounts");
        }
    }
}
