namespace Auth.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable // đảm bảo rằng các tài nguyên được giải phóng đúng cách sau khi sử dụng
    {
        Task SaveChangeAsync(Func<Task> action, CancellationToken cancellationToken = default);
        
        Task<T> SaveChangeAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default);
    }
}
