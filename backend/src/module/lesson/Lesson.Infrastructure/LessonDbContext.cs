namespace Lesson.Infrastructure;

public class LessonDbContext(DbContextOptions<LessonDbContext> options) : DbContext(options)
{
    public DbSet<CourseAggregate> Courses { get; set; } = null!;
    public DbSet<ExerciseAttemptAggregate> ExerciseAttempts { get; set; } = null!;
    public DbSet<UserTopicExerciseSessionItem> UserExerciseSessionItems { get; set; } = null!;
    public DbSet<TopicProgressAggregate> TopicProgresses { get; set; } = null!;
    public DbSet<UserTopicExerciseSessionAggregate> UserExerciseSessions { get; set; } = null!;
    public DbSet<TopicAggregate> Topics { get; set; } = null!;
    public DbSet<ExerciseAggregate> Exercises { get; set; } = null!;
    public DbSet<FlashcardAggregate> Flashcards { get; set; } = null!;

    public DbSet<LessonModuleOutboxMessage> LessonOutboxMessages { get; set; } = null!;
    // hàm này được gọi khi EF Core xây dựng mô hình dữ liệu (model) từ các class C#.
        // Chúng ta sẽ cấu hình các mối quan hệ, ràng buộc, kiểu dữ liệu,... ở đây.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Tự động quét và áp dụng TẤT CẢ các class IEntityTypeConfiguration trong assembly này
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly()); // assembly này là assembly chứa LessonDbContext,
                                                                                           // tức là sẽ quét tất cả class trong thư mục Data/Config/
                                                                                           // sử dụng reflection để tìm class nào implement IEntityTypeConfiguration<T> và áp dụng chúng.
                                                                                           //modelBuilder.Entity<Todo>()
                                                                                           //    .HasMany(t => t.Items)
                                                                                           //    .WithOne()
                                                                                           //    .HasForeignKey(i => i.CategoryId)
                                                                                           //    .OnDelete(DeleteBehavior.Cascade);

        }
}