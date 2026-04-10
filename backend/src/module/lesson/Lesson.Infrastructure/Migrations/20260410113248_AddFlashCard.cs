using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lesson.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFlashCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTopicExerciseSessions_Topics_TopicId",
                table: "UserTopicExerciseSessions");

            migrationBuilder.CreateTable(
                name: "Flashcards",
                columns: table => new
                {
                    FlashcardId = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    TopicId = table.Column<Guid>(type: "uuid", nullable: false),
                    FrontTextChinese = table.Column<string>(type: "text", nullable: false),
                    Pinyin = table.Column<string>(type: "text", nullable: false),
                    MeaningVi = table.Column<string>(type: "text", nullable: false),
                    MeaningEn = table.Column<string>(type: "text", nullable: true),
                    PhraseType = table.Column<int>(type: "integer", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    AudioUrl = table.Column<string>(type: "text", nullable: true),
                    HskLevel = table.Column<int>(type: "integer", nullable: true),
                    IsHskCore = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExampleSentenceChinese = table.Column<string>(type: "text", nullable: true),
                    ExampleSentencePinyin = table.Column<string>(type: "text", nullable: true),
                    ExampleSentenceMeaningVi = table.Column<string>(type: "text", nullable: true),
                    Radical = table.Column<string>(type: "text", nullable: true),
                    StrokeCount = table.Column<int>(type: "integer", nullable: true),
                    TraditionalForm = table.Column<string>(type: "text", nullable: true),
                    StrokeOrderJson = table.Column<JsonDocument>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flashcards", x => x.FlashcardId);
                    table.ForeignKey(
                        name: "FK_Flashcards_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Flashcards_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "TopicId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Flashcards_CourseId",
                table: "Flashcards",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Flashcards_CourseId_TopicId_OrderIndex",
                table: "Flashcards",
                columns: new[] { "CourseId", "TopicId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Flashcards_FlashcardId",
                table: "Flashcards",
                column: "FlashcardId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Flashcards_OrderIndex",
                table: "Flashcards",
                column: "OrderIndex");

            migrationBuilder.CreateIndex(
                name: "IX_Flashcards_TopicId",
                table: "Flashcards",
                column: "TopicId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTopicExerciseSessions_Topics_TopicId",
                table: "UserTopicExerciseSessions",
                column: "TopicId",
                principalTable: "Topics",
                principalColumn: "TopicId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTopicExerciseSessions_Topics_TopicId",
                table: "UserTopicExerciseSessions");

            migrationBuilder.DropTable(
                name: "Flashcards");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTopicExerciseSessions_Topics_TopicId",
                table: "UserTopicExerciseSessions",
                column: "TopicId",
                principalTable: "Topics",
                principalColumn: "TopicId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
