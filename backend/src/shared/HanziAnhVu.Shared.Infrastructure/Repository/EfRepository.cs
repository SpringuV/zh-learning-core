using System.Linq.Expressions;
using Ardalis.Specification.EntityFrameworkCore;
using HanziAnhVu.Shared.Domain;
using Microsoft.EntityFrameworkCore;

namespace HanziAnhVu.Shared.Infrastructure.Repository
{
    public class EfRepository<T>(DbContext dbContext) : RepositoryBase<T>(dbContext), IReadRepository<T>, IRepository<T> where T : class, IAggregateRoot
    {
    }
}
