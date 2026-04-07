using Classroom.Domain.Entities.Assignment;

namespace Classroom.Domain.Interface;

/// <summary>
/// IAssignmentRepository - Write operations (CQRS Write Side)
/// For read operations, use IAssignmentSearchService (Elasticsearch)
/// </summary>
public interface IAssignmentRepository
{
    /// <summary>
    /// Get assignment by id (for aggregate loading only)
    /// </summary>
    Task<AssignmentAggregate?> GetByIdAsync(Guid assignmentId);

    /// <summary>
    /// Save assignment (insert or update)
    /// Publishes domain events to Outbox
    /// </summary>
    Task SaveAsync(AssignmentAggregate assignment);

    /// <summary>
    /// Delete assignment
    /// </summary>
    Task DeleteAsync(Guid assignmentId);
}
