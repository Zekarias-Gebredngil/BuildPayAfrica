using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildPay.IdentityService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContractorVerifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LicenceNumber = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractorVerifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContractorVerificationDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractorVerificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractorVerificationDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractorVerificationDocuments_ContractorVerifications_Con~",
                        column: x => x.ContractorVerificationId,
                        principalTable: "ContractorVerifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractorVerificationDocuments_ContractorVerificationId",
                table: "ContractorVerificationDocuments",
                column: "ContractorVerificationId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractorVerifications_Status",
                table: "ContractorVerifications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ContractorVerifications_UserId",
                table: "ContractorVerifications",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractorVerificationDocuments");

            migrationBuilder.DropTable(
                name: "ContractorVerifications");
        }
    }
}
