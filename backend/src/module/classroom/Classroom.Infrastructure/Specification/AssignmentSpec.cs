using Ardalis.Specification;
using Classroom.Domain.Entities.Assignment;

namespace Classroom.Infrastructure.Specification;

public class GetAssignmentByIdWithChildrenSpec : Specification<AssignmentAggregate>
{
    public GetAssignmentByIdWithChildrenSpec(Guid assignmentId)
    {
        Query.Where(a => a.AssignmentId == assignmentId)
             .Include(a => (IEnumerable<AssignmentExercise>)a.Exercises)
             .Include(a => (IEnumerable<AssignmentRecipient>)a.Recipients);
    }
}
