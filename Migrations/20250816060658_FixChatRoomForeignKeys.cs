using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Freelancing.Migrations
{
    /// <inheritdoc />
    public partial class FixChatRoomForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatRooms_MentorshipMatches_MentorshipMatchId",
                table: "ChatRooms");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatRooms_Projects_ProjectId",
                table: "ChatRooms");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRooms_MentorshipMatches_MentorshipMatchId",
                table: "ChatRooms",
                column: "MentorshipMatchId",
                principalTable: "MentorshipMatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRooms_Projects_ProjectId",
                table: "ChatRooms",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatRooms_MentorshipMatches_MentorshipMatchId",
                table: "ChatRooms");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatRooms_Projects_ProjectId",
                table: "ChatRooms");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRooms_MentorshipMatches_MentorshipMatchId",
                table: "ChatRooms",
                column: "MentorshipMatchId",
                principalTable: "MentorshipMatches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRooms_Projects_ProjectId",
                table: "ChatRooms",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");
        }
    }
}
