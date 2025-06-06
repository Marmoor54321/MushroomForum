using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MushroomForum.Migrations
{
    /// <inheritdoc />
    public partial class NotesAndSpotsUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "MushroomSpots",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "MushroomNotes",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MushroomSpots_UserId",
                table: "MushroomSpots",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MushroomNotes_UserId",
                table: "MushroomNotes",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MushroomNotes_AspNetUsers_UserId",
                table: "MushroomNotes",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MushroomSpots_AspNetUsers_UserId",
                table: "MushroomSpots",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MushroomNotes_AspNetUsers_UserId",
                table: "MushroomNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_MushroomSpots_AspNetUsers_UserId",
                table: "MushroomSpots");

            migrationBuilder.DropIndex(
                name: "IX_MushroomSpots_UserId",
                table: "MushroomSpots");

            migrationBuilder.DropIndex(
                name: "IX_MushroomNotes_UserId",
                table: "MushroomNotes");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "MushroomSpots");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "MushroomNotes");
        }
    }
}
