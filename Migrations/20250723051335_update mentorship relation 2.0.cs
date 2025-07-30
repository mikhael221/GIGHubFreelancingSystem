using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Freelancing.Migrations
{
    /// <inheritdoc />
    public partial class updatementorshiprelation20 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAccounts_PeerMentorships_MentorshipId",
                table: "UserAccounts");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAccounts_PeerMentorships_MentorshipId",
                table: "UserAccounts",
                column: "MentorshipId",
                principalTable: "PeerMentorships",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAccounts_PeerMentorships_MentorshipId",
                table: "UserAccounts");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAccounts_PeerMentorships_MentorshipId",
                table: "UserAccounts",
                column: "MentorshipId",
                principalTable: "PeerMentorships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
