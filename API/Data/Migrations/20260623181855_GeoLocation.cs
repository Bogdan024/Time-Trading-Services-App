using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Data.Migrations
{
    /// <inheritdoc />
    public partial class GeoLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FormattedAddress",
                table: "TimeTasks",
                type: "TEXT",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "TimeTasks",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "TimeTasks",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlaceId",
                table: "TimeTasks",
                type: "TEXT",
                maxLength: 150,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FormattedAddress",
                table: "TimeTasks");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "TimeTasks");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "TimeTasks");

            migrationBuilder.DropColumn(
                name: "PlaceId",
                table: "TimeTasks");
        }
    }
}
