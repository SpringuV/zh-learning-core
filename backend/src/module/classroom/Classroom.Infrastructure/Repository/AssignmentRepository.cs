using Ardalis.Specification.EntityFrameworkCore;
using Classroom.Domain.Entities.Assignment;
using Classroom.Domain.Interface;
using Classroom.Infrastructure.Specification;
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
        var spec = new GetAssignmentByIdWithChildrenSpec(assignmentId);
        return await _context.Assignments.WithSpecification(spec).FirstOrDefaultAsync();
    }

    public async Task SaveAsync(AssignmentAggregate assignment)
    {
        // check exist
        var existingAssignment = await _context.Assignments.FindAsync(assignment.AssignmentId);
        
        if (existingAssignment == null)
        {
            // New assignment
            _context.Assignments.Add(assignment);
        }
        else
        {
            // Update existing assignment
            // update property values (title, description, due_date, etc.)
            _context.Entry(existingAssignment).CurrentValues.SetValues(assignment);
            
            // Handle child entities - exercises
            // load exercise of assignement
            var existingExercises = _context.Set<AssignmentExercise>()
                .Where(e => e.AssignmentId == assignment.AssignmentId)
                .ToList();
            // - Tìm exercises cần xóa (có trong DB nhưng không trong aggregate)
            var exercisesToRemove = existingExercises
                .Where(e => !assignment.Exercises.Any(x => x.Id == e.Id))
                .ToList();
            
            foreach (var exercise in exercisesToRemove)
            {
                _context.Set<AssignmentExercise>().Remove(exercise);
            }
            // - Thêm exercises mới (có trong aggregate nhưng không trong DB)
            foreach (var exercise in assignment.Exercises)
            {
                if (!existingExercises.Any(e => e.Id == exercise.Id))
                {
                    _context.Set<AssignmentExercise>().Add(exercise);
                }
            }

            // Handle child entities - recipients, GIỐNG EXERCISE
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
        /*
        DB hiện tại:
┌──────────────────────────┐
│ Assignment ID: 123       │
│ Exercises:               │
│  - Exercise A (ID: ex1)  │
│  - Exercise B (ID: ex2)  │
│  - Exercise C (ID: ex3)  │
└──────────────────────────┘

Aggregate (code gọi update):
┌──────────────────────────┐
│ Assignment ID: 123       │
│ Exercises:               │
│  - Exercise A (ID: ex1)  │
│  - Exercise C (ID: ex3)  │
│  - Exercise D (ID: ex4)  │ ← Bài mới
└──────────────────────────┘

Logic so sánh:
// Exercises trong DB
var existingExercises = [ex1, ex2, ex3]; 

// Exercises trong aggregate
var assignment.Exercises = [ex1, ex3, ex4];

// Tìm cái nào có trong DB nhưng KHÔNG trong aggregate
var exercisesToRemove = [ex2]; // ex2 không tồn tại trong aggregate nữa

// → XÓA ex2 khỏi DB
_context.Set<AssignmentExercise>().Remove(ex2);

// Tìm cái nào có trong aggregate nhưng KHÔNG trong DB
// → THÊM ex4 vào DB
_context.Set<AssignmentExercise>().Add(ex4);

// Kết quả sau sync:
// DB = [ex1, ex3, ex4] ✅ (giống aggregate)
        */
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
