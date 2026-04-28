using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lesson.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Fix2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TopicProgress_UserId_TopicId",
                table: "TopicProgress");

            migrationBuilder.CreateIndex(
                name: "IX_TopicProgress_UserId_TopicId",
                table: "TopicProgress",
                columns: new[] { "UserId", "TopicId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TopicProgress_UserId_TopicId",
                table: "TopicProgress");

            migrationBuilder.CreateIndex(
                name: "IX_TopicProgress_UserId_TopicId",
                table: "TopicProgress",
                columns: new[] { "UserId", "TopicId" },
                unique: true);
        }
    }
}
