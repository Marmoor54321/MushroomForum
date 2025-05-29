using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MushroomForum.Migrations
{
    /// <inheritdoc />
    public partial class MushroomSpotsRateAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "MushroomSpots",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rating",
                table: "MushroomSpots");
        }
    }
}
