using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Play.Catalog.Service.Entities;

namespace Play.Catalog.Service.Repositories
{
    public interface IRepository<T> where T : IEntity
    {
        Task CreateAsync(T T);
        Task DeleteAsync(Guid id);
        Task<T> GetByIdAsync(Guid id);
        Task<List<T>> GetListAsync();
        Task UpdateAsync(T T);
    }
}