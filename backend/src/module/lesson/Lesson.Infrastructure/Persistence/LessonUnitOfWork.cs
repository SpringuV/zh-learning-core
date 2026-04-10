namespace Lesson.Infrastructure.Persistence;

public sealed class LessonUnitOfWork(
    LessonDbContext dbContext,
    ILogger<EfUnitOfWork<LessonDbContext>> logger)
    : EfUnitOfWork<LessonDbContext>(dbContext, logger), ILessonUnitOfWork
{
}