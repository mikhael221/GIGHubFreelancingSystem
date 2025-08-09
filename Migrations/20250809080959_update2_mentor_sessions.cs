using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Freelancing.Migrations
{
    /// <inheritdoc />
    public partial class update2_mentor_sessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MentorshipSessions_ScheduledStartUtc_ScheduledEndUtc",
                table: "MentorshipSessions");

            migrationBuilder.DropColumn(
                name: "ScheduledEndUtc",
                table: "MentorshipSessions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledEndUtc",
                table: "MentorshipSessions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_MentorshipSessions_ScheduledStartUtc_ScheduledEndUtc",
                table: "MentorshipSessions",
                columns: new[] { "ScheduledStartUtc", "ScheduledEndUtc" });
        }
    }
}
