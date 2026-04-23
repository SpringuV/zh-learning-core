using Microsoft.EntityFrameworkCore.Design;

namespace Lesson.Infrastructure;

// file này lo phần tạo instance của LessonDbContext khi chạy lệnh migration, vì lúc này chưa có dependency injection để inject DbContext vào
public sealed class LessonDbContextDesignTimeFactory : IDesignTimeDbContextFactory<LessonDbContext>
{
    public LessonDbContext CreateDbContext(string[] args)
    {
        var options = DesignTimeDbContextOptionsFactory.CreateOptions<LessonDbContext>();
        return new LessonDbContext(options);
    }
}