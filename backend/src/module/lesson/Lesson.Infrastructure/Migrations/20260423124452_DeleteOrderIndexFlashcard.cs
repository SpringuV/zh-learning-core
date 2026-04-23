using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lesson.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DeleteOrderIndexFlashcard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Flashcards_CourseId_TopicId_OrderIndex",
                table: "Flashcards");

            migrationBuilder.DropIndex(
                name: "IX_Flashcards_OrderIndex",
                table: "Flashcards");

            migrationBuilder.DropColumn(
                name: "OrderIndex",
                table: "Flashcards");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderIndex",
                table: "Flashcards",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Flashcards_CourseId_TopicId_OrderIndex",
                table: "Flashcards",
                columns: new[] { "CourseId", "TopicId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Flashcards_OrderIndex",
                table: "Flashcards",
                column: "OrderIndex");
        }
    }
}
