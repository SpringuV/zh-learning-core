using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lesson.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TopicAddTotalExercise : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TotalExercises",
                table: "Topics",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalExercises",
                table: "Topics");
        }
    }
}
