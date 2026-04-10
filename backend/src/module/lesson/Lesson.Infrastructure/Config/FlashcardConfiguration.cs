namespace Lesson.Infrastructure.Config;

public class FlashcardConfiguration : IEntityTypeConfiguration<FlashcardAggregate>
{
    public void Configure(EntityTypeBuilder<FlashcardAggregate> builder)
    {
        builder.ToTable("Flashcards");
        builder.HasIndex(f => f.FlashcardId).IsUnique();
        builder.HasIndex(f => new { f.CourseId, f.TopicId, f.OrderIndex }).IsUnique(); // đảm bảo không có 2 flashcard nào trong cùng 1 topic có cùng order index
        builder.HasIndex(f => f.CourseId);
        builder.HasIndex(f => f.TopicId);
        builder.HasIndex(f => f.OrderIndex);

        // Key configuration
        builder.HasKey(f => f.FlashcardId);
        builder.Property(f => f.FlashcardId).IsRequired();
        
        // Required properties
        builder.Property(f => f.CourseId).IsRequired();
        builder.Property(f => f.TopicId).IsRequired();
        builder.Property(f => f.FrontTextChinese).IsRequired();
        builder.Property(f => f.Pinyin).IsRequired();
        builder.Property(f => f.MeaningVi).IsRequired();
        builder.Property(f => f.PhraseType).IsRequired();
        builder.Property(f => f.OrderIndex).IsRequired();
        builder.Property(f => f.CreatedAt).IsRequired();
        builder.Property(f => f.UpdatedAt).IsRequired();
        builder.Property(f => f.IsHskCore).IsRequired();
        // Optional properties
        builder.Property(f => f.MeaningEn).IsRequired(false);
        builder.Property(f => f.AudioUrl).IsRequired(false);
        builder.Property(f => f.HskLevel).IsRequired(false);
        builder.Property(f => f.ExampleSentenceChinese).IsRequired(false);
        builder.Property(f => f.ExampleSentencePinyin).IsRequired(false);
        builder.Property(f => f.ExampleSentenceMeaningVi).IsRequired(false);
        builder.Property(f => f.Radical).IsRequired(false);
        builder.Property(f => f.StrokeCount).IsRequired(false);
        builder.Property(f => f.TraditionalForm).IsRequired(false);
        builder.Property(f => f.StrokeOrderJson).IsRequired(false);
        // Relationship: Flashcard belongs to Course (same-module)
        builder.HasOne<CourseAggregate>()
            .WithMany()
            .HasForeignKey(f => f.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
        // Relationship: Flashcard belongs to Topic (same-module)
        builder.HasOne<TopicAggregate>()
            .WithMany()
            .HasForeignKey(f => f.TopicId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
