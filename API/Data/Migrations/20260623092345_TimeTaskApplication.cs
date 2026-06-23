using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Data.Migrations
{
    /// <inheritdoc />
    public partial class TimeTaskApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TimeTaskId = table.Column<int>(type: "INTEGER", nullable: false),
                    ApplicantMemberId = table.Column<string>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskApplications_Members_ApplicantMemberId",
                        column: x => x.ApplicantMemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskApplications_TimeTasks_TimeTaskId",
                        column: x => x.TimeTaskId,
                        principalTable: "TimeTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskApplications_ApplicantMemberId",
                table: "TaskApplications",
                column: "ApplicantMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskApplications_TimeTaskId_ApplicantMemberId",
                table: "TaskApplications",
                columns: new[] { "TimeTaskId", "ApplicantMemberId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskApplications");
        }
    }
}
