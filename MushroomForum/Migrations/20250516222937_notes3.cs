using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MushroomForum.Migrations
{
    /// <inheritdoc />
    public partial class notes3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Media_MushroomNotes_MushroomNoteId",
                table: "Media");

            migrationBuilder.AlterColumn<int>(
                name: "MushroomNoteId",
                table: "Media",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Media_MushroomNotes_MushroomNoteId",
                table: "Media",
                column: "MushroomNoteId",
                principalTable: "MushroomNotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Media_MushroomNotes_MushroomNoteId",
                table: "Media");

            migrationBuilder.AlterColumn<int>(
                name: "MushroomNoteId",
                table: "Media",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Media_MushroomNotes_MushroomNoteId",
                table: "Media",
                column: "MushroomNoteId",
                principalTable: "MushroomNotes",
                principalColumn: "Id");
        }
    }
}
