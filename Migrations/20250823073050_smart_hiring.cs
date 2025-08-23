using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Freelancing.Migrations
{
    /// <inheritdoc />
    public partial class smart_hiring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HiringOutcomes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FreelancerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WasSuccessful = table.Column<bool>(type: "bit", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HiringOutcomes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HiringOutcomes_FreelancerId",
                table: "HiringOutcomes",
                column: "FreelancerId");

            migrationBuilder.CreateIndex(
                name: "IX_HiringOutcomes_ProjectId",
                table: "HiringOutcomes",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_HiringOutcomes_RecordedAt",
                table: "HiringOutcomes",
                column: "RecordedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HiringOutcomes");
        }
    }
}
