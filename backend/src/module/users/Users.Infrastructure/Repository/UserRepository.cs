using MediatR;
using Microsoft.Extensions.Logging;
using Users.Domain;
using Users.Domain.Interface;

namespace Users.Infrastructure.Repository;

public class UserRepository(UserModuleDbContext dbContext, ILogger<UserRepository> logger, IPublisher publisher) : IUserRepository
{
    private readonly ILogger<UserRepository> _logger = logger;
    private readonly IPublisher _publisher = publisher;
    private readonly UserModuleDbContext _dbContext = dbContext;

    Task<UserAggregate?> IUserRepository.GetByIdAsync(Guid id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task SaveAsync(UserAggregate user, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<UserAggregate>> GetAllAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(UserAggregate user, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(UserAggregate user, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}