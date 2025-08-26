using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Freelancing.Migrations
{
    /// <inheritdoc />
    public partial class updateverification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AwsRekognitionResponse",
                table: "IdentityVerifications");

            migrationBuilder.DropColumn(
                name: "AwsTextractResponse",
                table: "IdentityVerifications");

            migrationBuilder.DropColumn(
                name: "FaceImagePath",
                table: "IdentityVerifications");

            migrationBuilder.DropColumn(
                name: "IdDocumentImagePath",
                table: "IdentityVerifications");

            migrationBuilder.DropColumn(
                name: "IdDocumentNumber",
                table: "IdentityVerifications");

            migrationBuilder.DropColumn(
                name: "VerificationType",
                table: "IdentityVerifications");

            migrationBuilder.AddColumn<float>(
                name: "IdDocumentConfidence",
                table: "IdentityVerifications",
                type: "real",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdDocumentConfidence",
                table: "IdentityVerifications");

            migrationBuilder.AddColumn<string>(
                name: "AwsRekognitionResponse",
                table: "IdentityVerifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AwsTextractResponse",
                table: "IdentityVerifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FaceImagePath",
                table: "IdentityVerifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdDocumentImagePath",
                table: "IdentityVerifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdDocumentNumber",
                table: "IdentityVerifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerificationType",
                table: "IdentityVerifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
