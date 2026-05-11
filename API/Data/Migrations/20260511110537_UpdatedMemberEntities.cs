using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedMemberEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Availability",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "SkillsOffered",
                table: "Members");

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "Members",
                type: "TEXT",
                nullable: true);


            migrationBuilder.RenameColumn(
                name: "LastActive",
                table: "Members",
                newName: "LastActiveAtUtc");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Members",
                newName: "AvatarUrl");

            migrationBuilder.DropColumn(
                name: "HelpNeeded",
                table: "Members");

            migrationBuilder.RenameColumn(
                name: "Bio",
                table: "Members",
                newName: "About");


            migrationBuilder.RenameColumn(
                name: "Created",
                table: "Members",
                newName: "CreatedAtUtc");

            migrationBuilder.AddColumn<bool>(
                name: "IsProfilePublic",
                table: "Members",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "MemberAvailabilitySlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MemberId = table.Column<string>(type: "TEXT", nullable: false),
                    DayOfWeek = table.Column<int>(type: "INTEGER", nullable: false),
                    StartHour = table.Column<int>(type: "INTEGER", nullable: false),
                    EndHour = table.Column<int>(type: "INTEGER", nullable: false),
                    Mode = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberAvailabilitySlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberAvailabilitySlots_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MemberNeeds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MemberId = table.Column<string>(type: "TEXT", nullable: false),
                    ServiceCategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    Note = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberNeeds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberNeeds_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MemberNeeds_ServiceCategories_ServiceCategoryId",
                        column: x => x.ServiceCategoryId,
                        principalTable: "ServiceCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MemberSkills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MemberId = table.Column<string>(type: "TEXT", nullable: false),
                    ServiceCategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    Note = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberSkills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberSkills_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MemberSkills_ServiceCategories_ServiceCategoryId",
                        column: x => x.ServiceCategoryId,
                        principalTable: "ServiceCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MemberAvailabilitySlots_MemberId",
                table: "MemberAvailabilitySlots",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberNeeds_MemberId_ServiceCategoryId",
                table: "MemberNeeds",
                columns: new[] { "MemberId", "ServiceCategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MemberNeeds_ServiceCategoryId",
                table: "MemberNeeds",
                column: "ServiceCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberSkills_MemberId_ServiceCategoryId",
                table: "MemberSkills",
                columns: new[] { "MemberId", "ServiceCategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MemberSkills_ServiceCategoryId",
                table: "MemberSkills",
                column: "ServiceCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCategories_Key",
                table: "ServiceCategories",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemberAvailabilitySlots");

            migrationBuilder.DropTable(
                name: "MemberNeeds");

            migrationBuilder.DropTable(
                name: "MemberSkills");

            migrationBuilder.DropTable(
                name: "ServiceCategories");

            migrationBuilder.DropColumn(
                name: "IsProfilePublic",
                table: "Members");

            migrationBuilder.RenameColumn(
                name: "LastActiveAtUtc",
                table: "Members",
                newName: "LastActive");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "Members",
                newName: "Created");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "Members");

            migrationBuilder.AddColumn<string>(
                name: "SkillsOffered",
                table: "Members",
                type: "TEXT",
                nullable: true);


            migrationBuilder.RenameColumn(
                name: "AvatarUrl",
                table: "Members",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "About",
                table: "Members",
                newName: "Bio");

            migrationBuilder.AddColumn<string>(
                name: "HelpNeeded",
                table: "Members",
                type: "TEXT",
                nullable: true);


            migrationBuilder.AddColumn<string>(
                name: "Availability",
                table: "Members",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "Members",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Members",
                type: "TEXT",
                nullable: true);
        }
    }
}
