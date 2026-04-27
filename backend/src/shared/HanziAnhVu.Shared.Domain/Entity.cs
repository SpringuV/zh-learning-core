namespace HanziAnhVu.Shared.Domain;

/// <summary>
/// Base class for all entities in the domain.
/// Entities have identity and can be compared by identity.
/// Child entities in aggregates should inherit from this class.
/// </summary>
public abstract class Entity
{
    // Hash code caching for better performance in collections
    private int? _requestedHashCode;
    private Guid _id;

    public virtual Guid Id
    {
        get => _id;
        protected set => _id = value;
    }

    private List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent eventItem)
    {
        _domainEvents.Add(eventItem);
    }

    public void RemoveDomainEvent(IDomainEvent eventItem)
    {
        _domainEvents.Remove(eventItem);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public bool IsTransient() => Id == default;

    public override bool Equals(object? obj)
    {
        if (obj == null || obj is not Entity)
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (GetType() != obj.GetType())
            return false;

        var item = (Entity)obj;

        if (item.IsTransient() || IsTransient())
            return false;

        return item.Id == Id;
    }

    public override int GetHashCode()
    {
        if (!IsTransient())
        {
            _requestedHashCode ??= Id.GetHashCode() ^ 31; // XOR for random distribution
            return _requestedHashCode.Value;
        }

        return base.GetHashCode();
    }

    public static bool operator ==(Entity? left, Entity? right)
    {
        if (ReferenceEquals(left, null))
            return ReferenceEquals(right, null);

        return left.Equals(right);
    }

    public static bool operator !=(Entity? left, Entity? right)
    {
        return !(left == right);
    }
}

public sealed record ExerciseAttemptBatchScoredItemDTO(
    Guid AttemptId,
    Guid ExerciseId,
    SkillType SkillType,
    float Score,
    bool IsCorrect,
    string CorrectAnswer,
    IReadOnlyList<ExerciseOption> Options,
    ExerciseType ExerciseType,
    ExerciseDifficulty Difficulty,
    string Explanation,
    string? AudioUrl,
    string? ImageUrl,
    string Question,
    string Description,
    DateTime UpdatedAt
);