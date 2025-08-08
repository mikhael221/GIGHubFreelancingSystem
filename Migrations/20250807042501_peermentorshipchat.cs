using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Freelancing.Migrations
{
    /// <inheritdoc />
    public partial class peermentorshipchat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MentorshipChatMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MentorshipMatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    MessageType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsRead = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentorshipChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MentorshipChatMessages_MentorshipMatches_MentorshipMatchId",
                        column: x => x.MentorshipMatchId,
                        principalTable: "MentorshipMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MentorshipChatMessages_UserAccounts_SenderId",
                        column: x => x.SenderId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MentorshipChatFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentorshipChatFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MentorshipChatFiles_MentorshipChatMessages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "MentorshipChatMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MentorshipChatFiles_MessageId",
                table: "MentorshipChatFiles",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_MentorshipChatMessages_MentorshipMatchId",
                table: "MentorshipChatMessages",
                column: "MentorshipMatchId");

            migrationBuilder.CreateIndex(
                name: "IX_MentorshipChatMessages_MentorshipMatchId_SentAt",
                table: "MentorshipChatMessages",
                columns: new[] { "MentorshipMatchId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MentorshipChatMessages_SenderId",
                table: "MentorshipChatMessages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_MentorshipChatMessages_SentAt",
                table: "MentorshipChatMessages",
                column: "SentAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MentorshipChatFiles");

            migrationBuilder.DropTable(
                name: "MentorshipChatMessages");
        }
    }
}
