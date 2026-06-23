using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReportsAndModerationControl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ModerationStatus",
                table: "CommunityGroups",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "CommunityGroups",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAtUtc",
                table: "CommunityGroups",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewedByMemberId",
                table: "CommunityGroups",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ModerationReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TargetType = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetIntId = table.Column<int>(type: "INTEGER", nullable: true),
                    TargetStringId = table.Column<string>(type: "TEXT", nullable: true),
                    Reason = table.Column<int>(type: "INTEGER", nullable: false),
                    Details = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReviewedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModeratorNotes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ReporterMemberId = table.Column<string>(type: "TEXT", nullable: false),
                    ReviewedByMemberId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModerationReports_Members_ReporterMemberId",
                        column: x => x.ReporterMemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ModerationReports_Members_ReviewedByMemberId",
                        column: x => x.ReviewedByMemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityGroups_ModerationStatus",
                table: "CommunityGroups",
                column: "ModerationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityGroups_ReviewedByMemberId",
                table: "CommunityGroups",
                column: "ReviewedByMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationReports_ReporterMemberId",
                table: "ModerationReports",
                column: "ReporterMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationReports_ReviewedByMemberId",
                table: "ModerationReports",
                column: "ReviewedByMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationReports_Status",
                table: "ModerationReports",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationReports_TargetType_TargetIntId_TargetStringId",
                table: "ModerationReports",
                columns: new[] { "TargetType", "TargetIntId", "TargetStringId" });

            migrationBuilder.AddForeignKey(
                name: "FK_CommunityGroups_Members_ReviewedByMemberId",
                table: "CommunityGroups",
                column: "ReviewedByMemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommunityGroups_Members_ReviewedByMemberId",
                table: "CommunityGroups");

            migrationBuilder.DropTable(
                name: "ModerationReports");

            migrationBuilder.DropIndex(
                name: "IX_CommunityGroups_ModerationStatus",
                table: "CommunityGroups");

            migrationBuilder.DropIndex(
                name: "IX_CommunityGroups_ReviewedByMemberId",
                table: "CommunityGroups");

            migrationBuilder.DropColumn(
                name: "ModerationStatus",
                table: "CommunityGroups");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "CommunityGroups");

            migrationBuilder.DropColumn(
                name: "ReviewedAtUtc",
                table: "CommunityGroups");

            migrationBuilder.DropColumn(
                name: "ReviewedByMemberId",
                table: "CommunityGroups");
        }
    }
}
