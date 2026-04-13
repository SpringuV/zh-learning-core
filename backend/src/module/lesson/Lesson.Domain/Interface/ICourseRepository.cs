using Lesson.Domain.Entities;

namespace Lesson.Domain.Interface;

public interface ICourseRepository
{
    Task<CourseAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<CourseAggregate>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task<int?> GetMaxOrderIndexAsync(CancellationToken ct = default);
    Task AddAsync(CourseAggregate course, CancellationToken ct = default);
    Task UpdateAsync(CourseAggregate course, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    Task UpdateRangeAsync(IEnumerable<CourseAggregate> courses, CancellationToken ct = default);

}