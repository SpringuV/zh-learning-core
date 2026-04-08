namespace Classroom.Infrastructure.Config;
public class AssignmentExerciseConfiguration : IEntityTypeConfiguration<AssignmentExercise>
{
    public void Configure(EntityTypeBuilder<AssignmentExercise> builder)
    {
        builder.ToTable("AssignmentExercises");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.AssignmentId).IsRequired();
        builder.Property(e => e.ExerciseId).IsRequired();
        builder.Property(e => e.OrderIndex).IsRequired();
    }
}