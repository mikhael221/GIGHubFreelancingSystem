using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Freelancing.Migrations
{
    /// <inheritdoc />
    public partial class deliverables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Deliverables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmittedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SubmittedFilesPaths = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RepositoryLinks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    PreviousVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deliverables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deliverables_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Deliverables_Deliverables_PreviousVersionId",
                        column: x => x.PreviousVersionId,
                        principalTable: "Deliverables",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Deliverables_UserAccounts_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Deliverables_UserAccounts_SubmittedByUserId",
                        column: x => x.SubmittedByUserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Deliverables_ContractId",
                table: "Deliverables",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_Deliverables_PreviousVersionId",
                table: "Deliverables",
                column: "PreviousVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Deliverables_ReviewedByUserId",
                table: "Deliverables",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Deliverables_Status",
                table: "Deliverables",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Deliverables_SubmittedAt",
                table: "Deliverables",
                column: "SubmittedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Deliverables_SubmittedByUserId",
                table: "Deliverables",
                column: "SubmittedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Deliverables");
        }
    }
}
