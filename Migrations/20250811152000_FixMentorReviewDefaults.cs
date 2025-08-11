using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Freelancing.Migrations
{
    /// <inheritdoc />
    public partial class FixMentorReviewDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add default value for IsAnonymous column
            migrationBuilder.AlterColumn<bool>(
                name: "IsAnonymous",
                table: "MentorReviews",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: false);

            // Add default value for CreatedAt column
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "MentorReviews",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert CreatedAt column
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "MentorReviews",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: false,
                oldDefaultValueSql: "GETUTCDATE()");

            // Revert IsAnonymous column
            migrationBuilder.AlterColumn<bool>(
                name: "IsAnonymous",
                table: "MentorReviews",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: false,
                oldDefaultValue: false);
        }
    }
}
