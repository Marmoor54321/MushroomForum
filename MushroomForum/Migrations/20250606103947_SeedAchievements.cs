using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MushroomForum.Migrations
{
    /// <inheritdoc />
    public partial class SeedAchievements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AchievementTypes",
                columns: new[] { "Id", "Description", "ExperienceReward", "Name", "UnlocksAvatarIcon" },
                values: new object[,]
                {
                    { 1, "Dodaj pierwszy post na forum", 10, "FirstPost", null },
                    { 2, "Otrzymaj pierwsze polubienie pod swoim postem", 10, "FirstLikeReceived", null },
                    { 3, "Zdobądź 5 punktów w quizie", 40, "Quiz5Points", "Quiz5Points.png" },
                    { 4, "Dodaj pierwszego znajomego", 40, "FirstFriend", "FirstFriend.png" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AchievementTypes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AchievementTypes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AchievementTypes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "AchievementTypes",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
