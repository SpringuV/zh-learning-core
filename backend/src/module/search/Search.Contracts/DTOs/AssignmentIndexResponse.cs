namespace Search.Contracts.DTOs;

public sealed record AssignmentIndexResponse(
    Guid AssignmentId, 
    DateTime IndexedAt);
