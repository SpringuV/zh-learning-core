using Ardalis.Specification;
using HanziAnhVu.Shared.Domain;

namespace HanziAnhVu.Shared.Infrastructure;

public interface IReadRepository<T> : IReadRepositoryBase<T> where T : class, IAggregateRoot
{
}
