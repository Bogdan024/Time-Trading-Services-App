using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Data.Migrations
{
    /// <inheritdoc />
    public partial class GroupsMessagesFirstStage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommunityGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Theme = table.Column<string>(type: "TEXT", maxLength: 60, nullable: true),
                    City = table.Column<string>(type: "TEXT", maxLength: 80, nullable: true),
                    CountryCode = table.Column<string>(type: "TEXT", maxLength: 2, nullable: true),
                    IsPublic = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OwnerMemberId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommunityGroups_Members_OwnerMemberId",
                        column: x => x.OwnerMemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CommunityGroupMembers",
                columns: table => new
                {
                    CommunityGroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    MemberId = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    JoinedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityGroupMembers", x => new { x.CommunityGroupId, x.MemberId });
                    table.ForeignKey(
                        name: "FK_CommunityGroupMembers_CommunityGroups_CommunityGroupId",
                        column: x => x.CommunityGroupId,
                        principalTable: "CommunityGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommunityGroupMembers_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_GroupId",
                table: "Conversations",
                column: "GroupId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_Type_GroupId",
                table: "Conversations",
                columns: new[] { "Type", "GroupId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommunityGroupMembers_MemberId",
                table: "CommunityGroupMembers",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityGroups_Name",
                table: "CommunityGroups",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommunityGroups_OwnerMemberId",
                table: "CommunityGroups",
                column: "OwnerMemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_CommunityGroups_GroupId",
                table: "Conversations",
                column: "GroupId",
                principalTable: "CommunityGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_CommunityGroups_GroupId",
                table: "Conversations");

            migrationBuilder.DropTable(
                name: "CommunityGroupMembers");

            migrationBuilder.DropTable(
                name: "CommunityGroups");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_GroupId",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_Type_GroupId",
                table: "Conversations");
        }
    }
}
