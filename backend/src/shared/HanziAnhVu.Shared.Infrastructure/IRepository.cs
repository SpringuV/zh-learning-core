using Ardalis.Specification;
using HanziAnhVu.Shared.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace HanziAnhVu.Shared.Infrastructure
{
    public interface IRepository<T> : IRepositoryBase<T> where T : class, IAggregateRoot { }
}
