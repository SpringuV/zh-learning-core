using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lesson.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSessionProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExerciseAttempts_SessionItemId",
                table: "ExerciseAttempts");

            migrationBuilder.DropColumn(
                name: "SessionItemId",
                table: "ExerciseAttempts");

            migrationBuilder.AddColumn<int>(
                name: "HskLevel",
                table: "UserTopicExerciseSessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ScoreListening",
                table: "UserTopicExerciseSessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ScoreReading",
                table: "UserTopicExerciseSessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ScoreWriting",
                table: "UserTopicExerciseSessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalCorrect",
                table: "UserTopicExerciseSessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalWrong",
                table: "UserTopicExerciseSessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SkillType",
                table: "ExerciseAttempts",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HskLevel",
                table: "UserTopicExerciseSessions");

            migrationBuilder.DropColumn(
                name: "ScoreListening",
                table: "UserTopicExerciseSessions");

            migrationBuilder.DropColumn(
                name: "ScoreReading",
                table: "UserTopicExerciseSessions");

            migrationBuilder.DropColumn(
                name: "ScoreWriting",
                table: "UserTopicExerciseSessions");

            migrationBuilder.DropColumn(
                name: "TotalCorrect",
                table: "UserTopicExerciseSessions");

            migrationBuilder.DropColumn(
                name: "TotalWrong",
                table: "UserTopicExerciseSessions");

            migrationBuilder.DropColumn(
                name: "SkillType",
                table: "ExerciseAttempts");

            migrationBuilder.AddColumn<Guid>(
                name: "SessionItemId",
                table: "ExerciseAttempts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseAttempts_SessionItemId",
                table: "ExerciseAttempts",
                column: "SessionItemId");
        }
    }
}
