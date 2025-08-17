using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Freelancing.Migrations
{
    /// <inheritdoc />
    public partial class AddContractTermination : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContractTerminations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TerminationReason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TerminationDetails = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    RequestedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedByUserRole = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinalPayment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
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
                    TerminationTerms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SettlementDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SettlementNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentSize = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractTerminations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractTerminations_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContractTerminationAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractTerminationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IPAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractTerminationAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractTerminationAuditLogs_ContractTerminations_ContractTerminationId",
                        column: x => x.ContractTerminationId,
                        principalTable: "ContractTerminations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContractTerminationAuditLogs_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractTerminationAuditLogs_ContractTerminationId",
                table: "ContractTerminationAuditLogs",
                column: "ContractTerminationId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractTerminationAuditLogs_Timestamp",
                table: "ContractTerminationAuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_ContractTerminationAuditLogs_UserId",
                table: "ContractTerminationAuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractTerminations_ContractId",
                table: "ContractTerminations",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractTerminations_RequestedAt",
                table: "ContractTerminations",
                column: "RequestedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ContractTerminations_RequestedByUserId",
                table: "ContractTerminations",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractTerminations_Status",
                table: "ContractTerminations",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractTerminationAuditLogs");

            migrationBuilder.DropTable(
                name: "ContractTerminations");
        }
    }
}
