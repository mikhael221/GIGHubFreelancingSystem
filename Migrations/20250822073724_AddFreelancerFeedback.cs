using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Freelancing.Migrations
{
    /// <inheritdoc />
    public partial class AddFreelancerFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FreelancerFeedbacks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcceptBidId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FreelancerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    WouldRecommend = table.Column<bool>(type: "bit", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FreelancerFeedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FreelancerFeedbacks_Biddings_AcceptBidId",
                        column: x => x.AcceptBidId,
                        principalTable: "Biddings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FreelancerFeedbacks_UserAccounts_FreelancerId",
                        column: x => x.FreelancerId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FreelancerFeedbacks_AcceptBidId",
                table: "FreelancerFeedbacks",
                column: "AcceptBidId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FreelancerFeedbacks_CreatedAt",
                table: "FreelancerFeedbacks",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_FreelancerFeedbacks_FreelancerId",
                table: "FreelancerFeedbacks",
                column: "FreelancerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FreelancerFeedbacks");
        }
    }
}
