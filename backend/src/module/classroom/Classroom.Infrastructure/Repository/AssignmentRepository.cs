using Classroom.Domain.Entities.Assignment;
using Classroom.Domain.Interface;
using Microsoft.EntityFrameworkCore;

namespace Classroom.Infrastructure.Repository;

/// <summary>
/// AssignmentRepository - Write-only operations (CQRS Write Side)
/// For read operations, use AssignmentSearchService (Elasticsearch)
/// </summary>
public class AssignmentRepository(ClassroomDbContext context) : IAssignmentRepository
{
    private readonly ClassroomDbContext _context = context;

    public async Task<AssignmentAggregate?> GetByIdAsync(Guid assignmentId)
    {
        return await _context.Assignments
            .Include(a => (IEnumerable<AssignmentExercise>)a.Exercises)
            .Include(a => (IEnumerable<AssignmentRecipient>)a.Recipients)
            .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);
    }

    public async Task SaveAsync(AssignmentAggregate assignment)
    {
        var existingAssignment = await _context.Assignments.FindAsync(assignment.AssignmentId);
        
        if (existingAssignment == null)
        {
            // New assignment
            _context.Assignments.Add(assignment);
        }
        else
        {
            // Update existing assignment
            _context.Entry(existingAssignment).CurrentValues.SetValues(assignment);
            
            // Handle child entities - exercises
            var existingExercises = _context.Set<AssignmentExercise>()
                .Where(e => e.AssignmentId == assignment.AssignmentId)
                .ToList();
            
            var exercisesToRemove = existingExercises
                .Where(e => !assignment.Exercises.Any(x => x.Id == e.Id))
                .ToList();
            
            foreach (var exercise in exercisesToRemove)
            {
                _context.Set<AssignmentExercise>().Remove(exercise);
            }
            
            foreach (var exercise in assignment.Exercises)
            {
                if (!existingExercises.Any(e => e.Id == exercise.Id))
                {
                    _context.Set<AssignmentExercise>().Add(exercise);
                }
            }

            // Handle child entities - recipients
            var existingRecipients = _context.Set<AssignmentRecipient>()
                .Where(r => r.AssignmentId == assignment.AssignmentId)
                .ToList();
            
            var recipientsToRemove = existingRecipients
                .Where(r => !assignment.Recipients.Any(x => x.Id == r.Id))
                .ToList();
            
            foreach (var recipient in recipientsToRemove)
            {
                _context.Set<AssignmentRecipient>().Remove(recipient);
            }
            
            foreach (var recipient in assignment.Recipients)
            {
                if (!existingRecipients.Any(r => r.Id == recipient.Id))
                {
                    _context.Set<AssignmentRecipient>().Add(recipient);
                }
            }
        }

        await _context.SaveChangesAsync();
        
        // TODO: Publish domain events to Outbox
        // This will be synced to Elasticsearch via event handler
    }

    public async Task DeleteAsync(Guid assignmentId)
    {
        var assignment = await _context.Assignments.FindAsync(assignmentId);
        if (assignment != null)
        {
            _context.Assignments.Remove(assignment);
            await _context.SaveChangesAsync();
            
            // TODO: Publish domain events to Outbox
        }
    }
}
