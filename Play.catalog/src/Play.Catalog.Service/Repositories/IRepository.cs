using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Play.Catalog.Service.Entities;

namespace Play.Catalog.Service.Repositories
{
    public interface IRepository
    {
        Task Create(Item entity);
        Task<Item> Get(Guid id);
        Task<IReadOnlyCollection<Item>> GetAll();
        Task Remove(Guid id);
        Task Update(Item entity);
    }
}