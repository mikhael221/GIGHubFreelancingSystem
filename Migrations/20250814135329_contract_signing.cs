using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Freelancing.Migrations
{
    /// <inheritdoc />
    public partial class contract_signing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BiddingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContractContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContractTemplateUsed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClientSignedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClientSignatureType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientSignatureData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientIPAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientUserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FreelancerSignedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FreelancerSignatureType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FreelancerSignatureData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FreelancerIPAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FreelancerUserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentTerms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeliverableRequirements = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RevisionPolicy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timeline = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentSize = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contracts_Biddings_BiddingId",
                        column: x => x.BiddingId,
                        principalTable: "Biddings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Contracts_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContractTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TemplateContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PreviewImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UsageCount = table.Column<int>(type: "int", nullable: false),
                    TemplateVersion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContractAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IPAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreviousStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewStatus = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractAuditLogs_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContractAuditLogs_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ContractRevisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RevisionNumber = table.Column<int>(type: "int", nullable: false),
                    RevisionContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RevisionNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    PreviousHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractRevisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractRevisions_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContractRevisions_UserAccounts_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractAuditLogs_ContractId",
                table: "ContractAuditLogs",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractAuditLogs_Timestamp",
                table: "ContractAuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_ContractAuditLogs_UserId",
                table: "ContractAuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractRevisions_ContractId_RevisionNumber",
                table: "ContractRevisions",
                columns: new[] { "ContractId", "RevisionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractRevisions_CreatedByUserId",
                table: "ContractRevisions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_BiddingId",
                table: "Contracts",
                column: "BiddingId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_CreatedAt",
                table: "Contracts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ProjectId",
                table: "Contracts",
                column: "ProjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_Status",
                table: "Contracts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ContractTemplates_Category",
                table: "ContractTemplates",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_ContractTemplates_IsActive",
                table: "ContractTemplates",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ContractTemplates_IsDefault",
                table: "ContractTemplates",
                column: "IsDefault");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractAuditLogs");

            migrationBuilder.DropTable(
                name: "ContractRevisions");

            migrationBuilder.DropTable(
                name: "ContractTemplates");

            migrationBuilder.DropTable(
                name: "Contracts");
        }
    }
}
