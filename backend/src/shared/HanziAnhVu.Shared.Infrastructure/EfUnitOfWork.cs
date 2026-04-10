using HanziAnhVu.Shared.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HanziAnhVu.Shared.Infrastructure;

// UnitOfWork implementation using Entity Framework Core, đảm bảo rằng tất cả các thay đổi trong một transaction được commit hoặc rollback cùng nhau.
// generict T để có thể sử dụng với bất kỳ DbContext nào, giúp tăng tính tái sử dụng của code.
// Dùng DbContext.Database.BeginTransactionAsync() thay vì TransactionScope vì TransactionScope không async-friendly với PostgreSQL
public class EfUnitOfWork<T>(T dbContext, ILogger<EfUnitOfWork<T>> logger) : IUnitOfWork where T : DbContext
{
    private readonly T _dbContext = dbContext;
    private readonly ILogger<EfUnitOfWork<T>> _logger = logger;

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task SaveChangeAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        // Use DbContext transaction instead of TransactionScope (better for async)
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            _logger.LogInformation("[EfUnitOfWork] Starting transaction - executing action");
            await action();
            
            _logger.LogInformation("[EfUnitOfWork] Action completed - calling SaveChangesAsync on DbContext {DbContextName}", typeof(T).Name);
            var rowsAffected = await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("[EfUnitOfWork] SaveChangesAsync SUCCESSFULLY completed - {RowsAffected} rows affected on {DbContextType}", rowsAffected, _dbContext.GetType().Name);
            
            cancellationToken.ThrowIfCancellationRequested();
            
            _logger.LogInformation("[EfUnitOfWork] Committing transaction");
            await transaction.CommitAsync(cancellationToken);
            _logger.LogInformation("[EfUnitOfWork] Transaction committed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EfUnitOfWork] EXCEPTION in SaveChangeAsync - Transaction will be rolled back. DbContext: {DbContextType}", _dbContext.GetType().Name);
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<TResult> SaveChangeAsync<TResult>(Func<Task<TResult>> action, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        // Use DbContext transaction instead of TransactionScope (better for async)
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            _logger.LogInformation("[EfUnitOfWork] Starting transaction (generic) - executing action");
            var result = await action();
            
            _logger.LogInformation("[EfUnitOfWork] Action completed - calling SaveChangesAsync on DbContext {DbContextName}", typeof(T).Name);
            var rowsAffected = await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("[EfUnitOfWork] SaveChangesAsync SUCCESSFULLY completed (generic) - {RowsAffected} rows affected on {DbContextType}", rowsAffected, _dbContext.GetType().Name);
            
            cancellationToken.ThrowIfCancellationRequested();
            
            _logger.LogInformation("[EfUnitOfWork] Committing transaction");
            await transaction.CommitAsync(cancellationToken);
            _logger.LogInformation("[EfUnitOfWork] Transaction committed successfully");
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EfUnitOfWork] EXCEPTION in SaveChangeAsync<T> - Transaction will be rolled back. DbContext: {DbContextType}", _dbContext.GetType().Name);
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
