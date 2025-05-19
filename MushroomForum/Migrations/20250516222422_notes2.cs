using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MushroomForum.Migrations
{
    /// <inheritdoc />
    public partial class notes2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "MushroomNotes");

            migrationBuilder.AddColumn<int>(
                name: "MushroomNoteId",
                table: "Media",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Media_MushroomNoteId",
                table: "Media",
                column: "MushroomNoteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Media_MushroomNotes_MushroomNoteId",
                table: "Media",
                column: "MushroomNoteId",
                principalTable: "MushroomNotes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Media_MushroomNotes_MushroomNoteId",
                table: "Media");

            migrationBuilder.DropIndex(
                name: "IX_Media_MushroomNoteId",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "MushroomNoteId",
                table: "Media");

            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "MushroomNotes",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
