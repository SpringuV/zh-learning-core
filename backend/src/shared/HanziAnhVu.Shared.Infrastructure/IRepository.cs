using Ardalis.Specification;
using HanziAnhVu.Shared.Domain;

namespace HanziAnhVu.Shared.Infrastructure;

public interface IRepository<T> : IRepositoryBase<T> where T : class, IAggregateRoot { }
