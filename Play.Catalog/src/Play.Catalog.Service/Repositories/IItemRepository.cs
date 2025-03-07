using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Play.Catalog.Service.Entities;

namespace Play.Catalog.Service.Repositories
{
    public interface IItemRepository
    {
        Task CreateAsync(Item item);
        Task DeleteAsync(Guid id);
        Task<Item> GetByIdAsync(Guid id);
        Task<List<Item>> GetItemsAsync();
        Task UpdateAsync(Item item);
    }
}