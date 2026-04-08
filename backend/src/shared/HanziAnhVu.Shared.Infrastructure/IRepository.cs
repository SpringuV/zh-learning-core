using System.Linq.Expressions;
using Ardalis.Specification;
using HanziAnhVu.Shared.Domain;

namespace HanziAnhVu.Shared.Infrastructure
{
    // Write repository — các method: AddAsync, UpdateAsync, DeleteAsync, GetByIdAsync, FirstOrDefaultAsync, ListAsync. T ph?i là IAggregateRoot
    public interface IRepository<T> : IRepositoryBase<T> where T : class, IAggregateRoot
    {
    }
}
