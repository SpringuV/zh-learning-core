using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lesson.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExerciseSessionSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentSequenceNo",
                table: "UserTopicExerciseSessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalExercises",
                table: "UserTopicExerciseSessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "SessionItemId",
                table: "ExerciseAttempts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserTopicExerciseSessionItems",
                columns: table => new
                {
                    SessionItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    SequenceNo = table.Column<int>(type: "integer", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    AttemptId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ViewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AnsweredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTopicExerciseSessionItems", x => x.SessionItemId);
                    table.ForeignKey(
                        name: "FK_UserTopicExerciseSessionItems_UserTopicExerciseSessions_Ses~",
                        column: x => x.SessionId,
                        principalTable: "UserTopicExerciseSessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseAttempts_SessionItemId",
                table: "ExerciseAttempts",
                column: "SessionItemId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTopicExerciseSessionItems_ExerciseId",
                table: "UserTopicExerciseSessionItems",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTopicExerciseSessionItems_SessionId",
                table: "UserTopicExerciseSessionItems",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTopicExerciseSessionItems_SessionId_OrderIndex",
                table: "UserTopicExerciseSessionItems",
                columns: new[] { "SessionId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserTopicExerciseSessionItems_SessionId_SequenceNo",
                table: "UserTopicExerciseSessionItems",
                columns: new[] { "SessionId", "SequenceNo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserTopicExerciseSessionItems");

            migrationBuilder.DropIndex(
                name: "IX_ExerciseAttempts_SessionItemId",
                table: "ExerciseAttempts");

            migrationBuilder.DropColumn(
                name: "CurrentSequenceNo",
                table: "UserTopicExerciseSessions");

            migrationBuilder.DropColumn(
                name: "TotalExercises",
                table: "UserTopicExerciseSessions");

            migrationBuilder.DropColumn(
                name: "SessionItemId",
                table: "ExerciseAttempts");
        }
    }
}
