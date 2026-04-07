using Auth.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Transactions;

namespace HanziAnhVu.Shared.Infrastructure;

// UnitOfWork implementation using Entity Framework Core, đảm bảo rằng tất cả các thay đổi trong một transaction được commit hoặc rollback cùng nhau.
// generict T để có thể sử dụng với bất kỳ DbContext nào, giúp tăng tính tái sử dụng của code.
public class EfUnitOfWork<T>(T dbContext) : IUnitOfWork where T : DbContext
{
    private readonly T _dbContext = dbContext;

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task SaveChangeAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // sử dụng TransactionScope để đảm bảo rằng tất cả các thao tác trong action được thực thi trong một transaction,
        // nếu có lỗi sẽ rollback lại toàn bộ thay đổi.
        // transactionScopeAsyncFlowOption.Enabled cho phép sử dụng TransactionScope trong các phương thức async,
        // đảm bảo rằng transaction vẫn hoạt động đúng cách khi có await.
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await action();
        await _dbContext.SaveChangesAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        transaction.Complete();
    }

    public async Task<T> SaveChangeAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var result = await action();
        await _dbContext.SaveChangesAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        transaction.Complete();
        return result;
    }
}
