namespace Users.Domain.Interface;

public interface IUserRepository
{
    Task<UserAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task SaveAsync(UserAggregate user, CancellationToken ct = default);
    Task<IEnumerable<UserAggregate>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(UserAggregate user, CancellationToken ct = default);
    Task UpdateAsync(UserAggregate user, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}