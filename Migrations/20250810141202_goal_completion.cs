using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Freelancing.Migrations
{
    /// <inheritdoc />
    public partial class goal_completion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Goals",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Goals",
                type: "int",
                nullable: false,
                defaultValue: 0);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MentorshipGoalCompletions");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Goals");
        }
    }
}
