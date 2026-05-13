using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Data.Migrations
{
    /// <inheritdoc />
    public partial class TimeTasksExchangeLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TimeTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    EstimatedHours = table.Column<int>(type: "INTEGER", nullable: false),
                    LocationMode = table.Column<int>(type: "INTEGER", nullable: false),
                    City = table.Column<string>(type: "TEXT", maxLength: 80, nullable: true),
                    CountryCode = table.Column<string>(type: "TEXT", maxLength: 2, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DueAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ServiceCategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PostedByMemberId = table.Column<string>(type: "TEXT", nullable: false),
                    AcceptedByMemberId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeTasks_Members_AcceptedByMemberId",
                        column: x => x.AcceptedByMemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimeTasks_Members_PostedByMemberId",
                        column: x => x.PostedByMemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimeTasks_ServiceCategories_ServiceCategoryId",
                        column: x => x.ServiceCategoryId,
                        principalTable: "ServiceCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimeTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TimeTaskId = table.Column<int>(type: "INTEGER", nullable: false),
                    FromMemberId = table.Column<string>(type: "TEXT", nullable: false),
                    ToMemberId = table.Column<string>(type: "TEXT", nullable: false),
                    Hours = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeTransactions_Members_FromMemberId",
                        column: x => x.FromMemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimeTransactions_Members_ToMemberId",
                        column: x => x.ToMemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimeTransactions_TimeTasks_TimeTaskId",
                        column: x => x.TimeTaskId,
                        principalTable: "TimeTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TimeTasks_AcceptedByMemberId",
                table: "TimeTasks",
                column: "AcceptedByMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeTasks_PostedByMemberId",
                table: "TimeTasks",
                column: "PostedByMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeTasks_ServiceCategoryId",
                table: "TimeTasks",
                column: "ServiceCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeTasks_Status",
                table: "TimeTasks",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TimeTransactions_FromMemberId",
                table: "TimeTransactions",
                column: "FromMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeTransactions_TimeTaskId",
                table: "TimeTransactions",
                column: "TimeTaskId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimeTransactions_ToMemberId",
                table: "TimeTransactions",
                column: "ToMemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TimeTransactions");

            migrationBuilder.DropTable(
                name: "TimeTasks");
        }
    }
}
