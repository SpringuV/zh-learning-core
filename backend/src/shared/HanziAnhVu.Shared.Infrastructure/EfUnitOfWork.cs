
using Auth.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HanziAnhVu.Shared.Infrastructure;

// UnitOfWork implementation using Entity Framework Core, đảm bảo rằng tất cả các thay đổi trong một transaction được commit hoặc rollback cùng nhau.
// generict T để có thể sử dụng với bất kỳ DbContext nào, giúp tăng tính tái sử dụng của code.
public class EfUnitOfWork<T> : IUnitOfWork where T : DbContext
{
    private readonly T _dbContext;

    public EfUnitOfWork(T dbContext)
    {
        _dbContext = dbContext;
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public async Task SaveChangeAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await action();
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<T> SaveChangeAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await action();
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
