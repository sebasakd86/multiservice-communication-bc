using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Play.Common
{
    public interface IRepository<T> where T : IEntity
    {
        Task Create(T entity);
        Task<T> Get(Guid id);
        Task<T> Get(Expression<Func<T, bool>> filter);
        Task<IReadOnlyCollection<T>> GetAll();
        Task<IReadOnlyCollection<T>> GetAll(Expression<Func<T, bool>> filter);
        Task Remove(Guid id);
        Task Update(Guid id, T entity);
    }
}