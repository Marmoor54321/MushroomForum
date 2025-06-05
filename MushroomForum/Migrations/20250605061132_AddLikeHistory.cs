using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MushroomForum.Migrations
{
    /// <inheritdoc />
    public partial class AddLikeHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LikeHistories",
                columns: table => new
                {
                    LikeHistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LikerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    LikedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LikeHistories", x => x.LikeHistoryId);
                    table.ForeignKey(
                        name: "FK_LikeHistories_AspNetUsers_LikerId",
                        column: x => x.LikerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LikeHistories_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "PostId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LikeHistories_LikerId",
                table: "LikeHistories",
                column: "LikerId");

            migrationBuilder.CreateIndex(
                name: "IX_LikeHistories_PostId",
                table: "LikeHistories",
                column: "PostId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LikeHistories");
        }
    }
}
