using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Freelancing.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Goals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GoalName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GoalDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IconSvg = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Goals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Photo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MentorshipId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserSkills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSkills", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IconSvg = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsRead = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RelatedUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsEncrypted = table.Column<bool>(type: "bit", nullable: false),
                    EncryptionMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EncryptedTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EncryptedMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PeerMentorships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeerMentorships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeerMentorships_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAccountSkills",
                columns: table => new
                {
                    UserAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserSkillId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccountSkills", x => new { x.UserAccountId, x.UserSkillId });
                    table.ForeignKey(
                        name: "FK_UserAccountSkills_UserAccounts_UserAccountId",
                        column: x => x.UserAccountId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAccountSkills_UserSkills_UserSkillId",
                        column: x => x.UserSkillId,
                        principalTable: "UserSkills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MentorshipMatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MentorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MenteeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MentorMentorshipId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MenteeMentorshipId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeclinedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentorshipMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MentorshipMatches_PeerMentorships_MenteeMentorshipId",
                        column: x => x.MenteeMentorshipId,
                        principalTable: "PeerMentorships",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MentorshipMatches_PeerMentorships_MentorMentorshipId",
                        column: x => x.MentorMentorshipId,
                        principalTable: "PeerMentorships",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MentorshipMatches_UserAccounts_MenteeId",
                        column: x => x.MenteeId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MentorshipMatches_UserAccounts_MentorId",
                        column: x => x.MentorId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MentorReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MentorshipMatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MentorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MenteeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    WouldRecommend = table.Column<bool>(type: "bit", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Strengths = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AreasForImprovement = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentorReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MentorReviews_MentorshipMatches_MentorshipMatchId",
                        column: x => x.MentorshipMatchId,
                        principalTable: "MentorshipMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MentorReviews_UserAccounts_MenteeId",
                        column: x => x.MenteeId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MentorReviews_UserAccounts_MentorId",
                        column: x => x.MentorId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                });

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
                name: "MentorshipGoalCompletions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MentorshipMatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GoalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CompletionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsCompletedByMentor = table.Column<bool>(type: "bit", nullable: false),
                    IsCompletedByMentee = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentorshipGoalCompletions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MentorshipGoalCompletions_Goals_GoalId",
                        column: x => x.GoalId,
                        principalTable: "Goals",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MentorshipGoalCompletions_MentorshipMatches_MentorshipMatchId",
                        column: x => x.MentorshipMatchId,
                        principalTable: "MentorshipMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MentorshipGoalCompletions_UserAccounts_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MentorshipSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MentorshipMatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScheduledStartUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TimeZone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentorshipSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MentorshipSessions_MentorshipMatches_MentorshipMatchId",
                        column: x => x.MentorshipMatchId,
                        principalTable: "MentorshipMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateTable(
                name: "Biddings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Budget = table.Column<int>(type: "int", nullable: false),
                    Delivery = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Proposal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsAccepted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Biddings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Biddings_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProjectDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Budget = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AcceptedBidId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_Biddings_AcceptedBidId",
                        column: x => x.AcceptedBidId,
                        principalTable: "Biddings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Projects_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Biddings_ProjectId",
                table: "Biddings",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Biddings_UserId_ProjectId",
                table: "Biddings",
                columns: new[] { "UserId", "ProjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MentorReviews_CreatedAt",
                table: "MentorReviews",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MentorReviews_MenteeId",
                table: "MentorReviews",
                column: "MenteeId");

            migrationBuilder.CreateIndex(
                name: "IX_MentorReviews_MentorId",
                table: "MentorReviews",
                column: "MentorId");

            migrationBuilder.CreateIndex(
                name: "IX_MentorReviews_MentorshipMatchId",
                table: "MentorReviews",
                column: "MentorshipMatchId",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_MentorshipGoalCompletions_CompletedAt",
                table: "MentorshipGoalCompletions",
                column: "CompletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MentorshipGoalCompletions_CompletedByUserId",
                table: "MentorshipGoalCompletions",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MentorshipGoalCompletions_GoalId",
                table: "MentorshipGoalCompletions",
                column: "GoalId");

            migrationBuilder.CreateIndex(
                name: "IX_MentorshipGoalCompletions_MentorshipMatchId_GoalId",
                table: "MentorshipGoalCompletions",
                columns: new[] { "MentorshipMatchId", "GoalId" });

            migrationBuilder.CreateIndex(
                name: "IX_MentorshipMatches_MatchedDate",
                table: "MentorshipMatches",
                column: "MatchedDate");

            migrationBuilder.CreateIndex(
                name: "IX_MentorshipMatches_MenteeId",
                table: "MentorshipMatches",
                column: "MenteeId");

            migrationBuilder.CreateIndex(
                name: "IX_MentorshipMatches_MenteeMentorshipId",
                table: "MentorshipMatches",
                column: "MenteeMentorshipId");

            migrationBuilder.CreateIndex(
                name: "IX_MentorshipMatches_MentorId_MenteeId",
                table: "MentorshipMatches",
                columns: new[] { "MentorId", "MenteeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MentorshipMatches_MentorMentorshipId",
                table: "MentorshipMatches",
                column: "MentorMentorshipId");

            migrationBuilder.CreateIndex(
                name: "IX_MentorshipMatches_Status",
                table: "MentorshipMatches",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MentorshipSessions_MentorshipMatchId",
                table: "MentorshipSessions",
                column: "MentorshipMatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAt",
                table: "Notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IsRead",
                table: "Notifications",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PeerMentorships_UserId",
                table: "PeerMentorships",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_AcceptedBidId",
                table: "Projects",
                column: "AcceptedBidId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_UserId",
                table: "Projects",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_Email",
                table: "UserAccounts",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_UserName",
                table: "UserAccounts",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAccountSkills_UserSkillId",
                table: "UserAccountSkills",
                column: "UserSkillId");

            migrationBuilder.AddForeignKey(
                name: "FK_Biddings_Projects_ProjectId",
                table: "Biddings",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Biddings_Projects_ProjectId",
                table: "Biddings");

            migrationBuilder.DropTable(
                name: "MentorReviews");

            migrationBuilder.DropTable(
                name: "MentorshipChatFiles");

            migrationBuilder.DropTable(
                name: "MentorshipGoalCompletions");

            migrationBuilder.DropTable(
                name: "MentorshipSessions");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "UserAccountSkills");

            migrationBuilder.DropTable(
                name: "MentorshipChatMessages");

            migrationBuilder.DropTable(
                name: "Goals");

            migrationBuilder.DropTable(
                name: "UserSkills");

            migrationBuilder.DropTable(
                name: "MentorshipMatches");

            migrationBuilder.DropTable(
                name: "PeerMentorships");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Biddings");

            migrationBuilder.DropTable(
                name: "UserAccounts");
        }
    }
}
