using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Data.Migrations
{
    /// <inheritdoc />
    public partial class Reviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MemberReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TimeTaskId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReviewerMemberId = table.Column<string>(type: "TEXT", nullable: false),
                    ReviewedMemberId = table.Column<string>(type: "TEXT", nullable: false),
                    Rating = table.Column<int>(type: "INTEGER", nullable: false),
                    Comment = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberReviews_Members_ReviewedMemberId",
                        column: x => x.ReviewedMemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberReviews_Members_ReviewerMemberId",
                        column: x => x.ReviewerMemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberReviews_TimeTasks_TimeTaskId",
                        column: x => x.TimeTaskId,
                        principalTable: "TimeTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MemberReviews_ReviewedMemberId",
                table: "MemberReviews",
                column: "ReviewedMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberReviews_ReviewerMemberId_TimeTaskId",
                table: "MemberReviews",
                columns: new[] { "ReviewerMemberId", "TimeTaskId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MemberReviews_TimeTaskId",
                table: "MemberReviews",
                column: "TimeTaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemberReviews");
        }
    }
}
