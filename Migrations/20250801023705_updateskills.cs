using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Freelancing.Migrations
{
    /// <inheritdoc />
    public partial class updateskills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserAccountUserSkill");

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

            migrationBuilder.CreateIndex(
                name: "IX_UserAccountSkills_UserSkillId",
                table: "UserAccountSkills",
                column: "UserSkillId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserAccountSkills");

            migrationBuilder.CreateTable(
                name: "UserAccountUserSkill",
                columns: table => new
                {
                    UserAccountsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserSkillsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccountUserSkill", x => new { x.UserAccountsId, x.UserSkillsId });
                    table.ForeignKey(
                        name: "FK_UserAccountUserSkill_UserAccounts_UserAccountsId",
                        column: x => x.UserAccountsId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAccountUserSkill_UserSkills_UserSkillsId",
                        column: x => x.UserSkillsId,
                        principalTable: "UserSkills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserAccountUserSkill_UserSkillsId",
                table: "UserAccountUserSkill",
                column: "UserSkillsId");
        }
    }
}
