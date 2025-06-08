using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MushroomForum.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyQuestTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyQuestTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Target = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyQuestTypes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "DailyQuestTypes",
                columns: new[] { "Id", "DayOfWeek", "Description", "QuestType", "Target" },
                values: new object[,]
                {
                    { 1, 1, "Polub 3 posty", "LikePosts", 3 },
                    { 2, 2, "Odpowiedz na 3 posty", "ReplyPosts", 3 },
                    { 3, 3, "Przeglądaj 5 postów", "ViewPosts", 5 },
                    { 4, 4, "Polub 3 posty", "LikePosts", 3 },
                    { 5, 5, "Odpowiedz na 3 posty", "ReplyPosts", 3 },
                    { 6, 6, "Przeglądaj 5 postów", "ViewPosts", 5 },
                    { 19, 0, "Rozwiąż quiz", "SolveQuiz", 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyQuestTypes");
        }
    }
}
