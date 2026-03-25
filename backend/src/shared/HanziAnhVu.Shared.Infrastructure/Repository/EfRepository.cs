using Ardalis.Specification.EntityFrameworkCore;
using HanziAnhVu.Shared.Domain;
using Microsoft.EntityFrameworkCore;

namespace HanziAnhVu.Shared.Infrastructure.Repository
{
    public class EfRepository<T>: RepositoryBase<T>, IReadRepository<T>, IRepository<T> where T : class, IAggregateRoot
    {
        public EfRepository(DbContext dbContext) : base(dbContext)
        {
        }

    }
}
