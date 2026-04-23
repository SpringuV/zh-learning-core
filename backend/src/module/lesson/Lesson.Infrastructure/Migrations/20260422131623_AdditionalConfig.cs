using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lesson.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdditionalConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Canonicalize exercise slugs to ExerciseId-based values to guarantee uniqueness
            // before creating IX_Exercises_Slug on legacy data.
            migrationBuilder.Sql(@"
UPDATE ""Exercises""
SET ""Slug"" = 'exercise-' || lower(replace(""ExerciseId""::text, '-', ''))
");

            migrationBuilder.Sql(@"
UPDATE ""Topics""
SET ""Slug"" = 'topic-' || lower(replace(""TopicId""::text, '-', ''))
WHERE ""Slug"" IS NULL OR btrim(""Slug"") = '';
");

            migrationBuilder.Sql(@"
UPDATE ""Courses""
SET ""Slug"" = 'course-' || lower(replace(""CourseId""::text, '-', ''))
WHERE ""Slug"" IS NULL OR btrim(""Slug"") = '';
");

            migrationBuilder.Sql(@"
WITH ranked AS (
    SELECT
        ""TopicId"",
        ""Slug"",
        ROW_NUMBER() OVER (PARTITION BY ""Slug"" ORDER BY ""CreatedAt"", ""TopicId"") AS rn
    FROM ""Topics""
), dups AS (
    SELECT ""TopicId""
    FROM ranked
    WHERE rn > 1
)
UPDATE ""Topics"" t
SET ""Slug"" = left(t.""Slug"", 220) || '-' || lower(replace(t.""TopicId""::text, '-', ''))
FROM dups
WHERE t.""TopicId"" = dups.""TopicId"";
");

            migrationBuilder.Sql(@"
WITH ranked AS (
    SELECT
        ""CourseId"",
        ""Slug"",
        ROW_NUMBER() OVER (PARTITION BY ""Slug"" ORDER BY ""CreatedAt"", ""CourseId"") AS rn
    FROM ""Courses""
), dups AS (
    SELECT ""CourseId""
    FROM ranked
    WHERE rn > 1
)
UPDATE ""Courses"" c
SET ""Slug"" = left(c.""Slug"", 220) || '-' || lower(replace(c.""CourseId""::text, '-', ''))
FROM dups
WHERE c.""CourseId"" = dups.""CourseId"";
");

            migrationBuilder.CreateIndex(
                name: "IX_Topics_Slug",
                table: "Topics",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Topics_TopicId",
                table: "Topics",
                column: "TopicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_ExerciseId",
                table: "Exercises",
                column: "ExerciseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_Slug",
                table: "Exercises",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_Slug",
                table: "Courses",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Topics_Slug",
                table: "Topics");

            migrationBuilder.DropIndex(
                name: "IX_Topics_TopicId",
                table: "Topics");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_ExerciseId",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_Slug",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Courses_Slug",
                table: "Courses");
        }
    }
}
