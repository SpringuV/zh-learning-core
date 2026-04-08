using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Classroom.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialClassroom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Assignments",
                columns: table => new
                {
                    AssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassroomId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Description = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    IsTimedAssignment = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    AssignmentType = table.Column<int>(type: "integer", nullable: false),
                    SkillFocus = table.Column<int>(type: "integer", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignments", x => x.AssignmentId);
                });

            migrationBuilder.CreateTable(
                name: "AssignmentSubmissions",
                columns: table => new
                {
                    SubmissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    SelectedDurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeadlineAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FinalizedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalScore = table.Column<float>(type: "real", nullable: true),
                    TeacherFeedback = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    GradedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentSubmissions", x => x.SubmissionId);
                });

            migrationBuilder.CreateTable(
                name: "ClassroomEnrollments",
                columns: table => new
                {
                    EnrollmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassroomId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: true),
                    EnrolledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassroomEnrollments", x => x.EnrollmentId);
                });

            migrationBuilder.CreateTable(
                name: "Classrooms",
                columns: table => new
                {
                    ClassroomId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Description = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    Slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HskLevel = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ScheduleInfo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Price = table.Column<float>(type: "real", nullable: false),
                    PriceCurrency = table.Column<string>(type: "text", nullable: false),
                    ClassroomStatus = table.Column<string>(type: "text", nullable: false),
                    StudentIds = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classrooms", x => x.ClassroomId);
                });

            migrationBuilder.CreateTable(
                name: "ClassroomStudents",
                columns: table => new
                {
                    ClassroomStudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassroomId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    AddedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassroomStudents", x => x.ClassroomStudentId);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessagesClassroomModule",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Payload = table.Column<JsonElement>(type: "jsonb", nullable: false),
                    OccurredOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Error = table.Column<string>(type: "text", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessagesClassroomModule", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssignmentExercise",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    AssignmentAggregateAssignmentId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentExercise", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignmentExercise_Assignments_AssignmentAggregateAssignmen~",
                        column: x => x.AssignmentAggregateAssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "AssignmentId");
                    table.ForeignKey(
                        name: "FK_AssignmentExercise_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "AssignmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssignmentRecipient",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentAggregateAssignmentId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentRecipient", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignmentRecipient_Assignments_AssignmentAggregateAssignme~",
                        column: x => x.AssignmentAggregateAssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "AssignmentId");
                    table.ForeignKey(
                        name: "FK_AssignmentRecipient_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "AssignmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubmissionAnswer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Answer = table.Column<string>(type: "text", nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    Feedback = table.Column<string>(type: "text", nullable: false),
                    Score = table.Column<float>(type: "real", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AssignmentSubmissionAggregateSubmissionId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmissionAnswer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubmissionAnswer_AssignmentSubmissions_AssignmentSubmission~",
                        column: x => x.AssignmentSubmissionAggregateSubmissionId,
                        principalTable: "AssignmentSubmissions",
                        principalColumn: "SubmissionId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentExercise_AssignmentAggregateAssignmentId",
                table: "AssignmentExercise",
                column: "AssignmentAggregateAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentExercise_AssignmentId",
                table: "AssignmentExercise",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentRecipient_AssignmentAggregateAssignmentId",
                table: "AssignmentRecipient",
                column: "AssignmentAggregateAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentRecipient_AssignmentId",
                table: "AssignmentRecipient",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_ClassroomId",
                table: "Assignments",
                column: "ClassroomId");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_ClassroomId_IsPublished",
                table: "Assignments",
                columns: new[] { "ClassroomId", "IsPublished" });

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_ClassroomId_TeacherId",
                table: "Assignments",
                columns: new[] { "ClassroomId", "TeacherId" });

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_DueDate",
                table: "Assignments",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_TeacherId",
                table: "Assignments",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentSubmissions_AssignmentId",
                table: "AssignmentSubmissions",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentSubmissions_AssignmentId_StudentId",
                table: "AssignmentSubmissions",
                columns: new[] { "AssignmentId", "StudentId" });

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentSubmissions_DeadlineAt",
                table: "AssignmentSubmissions",
                column: "DeadlineAt");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentSubmissions_StudentId",
                table: "AssignmentSubmissions",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassroomEnrollments_ClassroomId",
                table: "ClassroomEnrollments",
                column: "ClassroomId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassroomEnrollments_StudentId",
                table: "ClassroomEnrollments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassroomEnrollments_StudentId_ClassroomId",
                table: "ClassroomEnrollments",
                columns: new[] { "StudentId", "ClassroomId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Classrooms_ClassroomStatus",
                table: "Classrooms",
                column: "ClassroomStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Classrooms_Slug",
                table: "Classrooms",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Classrooms_TeacherId",
                table: "Classrooms",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassroomStudents_ClassroomId",
                table: "ClassroomStudents",
                column: "ClassroomId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassroomStudents_ClassroomId_StudentId",
                table: "ClassroomStudents",
                columns: new[] { "ClassroomId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassroomStudents_StudentId",
                table: "ClassroomStudents",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessagesClassroomModule_OccurredOnUtc",
                table: "OutboxMessagesClassroomModule",
                column: "OccurredOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessagesClassroomModule_ProcessedOnUtc",
                table: "OutboxMessagesClassroomModule",
                column: "ProcessedOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionAnswer_AssignmentSubmissionAggregateSubmissionId",
                table: "SubmissionAnswer",
                column: "AssignmentSubmissionAggregateSubmissionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssignmentExercise");

            migrationBuilder.DropTable(
                name: "AssignmentRecipient");

            migrationBuilder.DropTable(
                name: "ClassroomEnrollments");

            migrationBuilder.DropTable(
                name: "Classrooms");

            migrationBuilder.DropTable(
                name: "ClassroomStudents");

            migrationBuilder.DropTable(
                name: "OutboxMessagesClassroomModule");

            migrationBuilder.DropTable(
                name: "SubmissionAnswer");

            migrationBuilder.DropTable(
                name: "Assignments");

            migrationBuilder.DropTable(
                name: "AssignmentSubmissions");
        }
    }
}
