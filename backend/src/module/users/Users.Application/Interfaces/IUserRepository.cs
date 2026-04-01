using Users.Domain;

namespace Users.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<UserAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(UserAggregate user, CancellationToken cancellationToken = default);
        Task UpdateAsync(UserAggregate user, CancellationToken cancellationToken = default);
    }
}
