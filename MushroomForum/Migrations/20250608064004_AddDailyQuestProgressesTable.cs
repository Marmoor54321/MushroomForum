using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MushroomForum.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyQuestProgressesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyQuestProgresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QuestType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Progress = table.Column<int>(type: "int", nullable: false),
                    Completed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyQuestProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyQuestProgresses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyQuestProgresses_UserId",
                table: "DailyQuestProgresses",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyQuestProgresses");
        }
    }
}
