using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Play.Catalog.Service.Entities;

namespace Play.Catalog.Service.Repositories
{

    public class ItemRepository : IItemRepository
    {
        private const string collectionName = "items";
        private readonly IMongoCollection<Item> collection;
        private readonly FilterDefinitionBuilder<Item> filterBuilder = Builders<Item>.Filter;

        public ItemRepository(IMongoDatabase db)
        {
            // var mongoClient = new MongoClient("mongodb://localhost:27017");
            // var db = mongoClient.GetDatabase("Catalog");
            collection = db.GetCollection<Item>(collectionName);

        }

        public async Task<List<Item>> GetItemsAsync()
        {
            return await collection.Find(filterBuilder.Empty).ToListAsync();
        }

        public async Task<Item?> GetByIdAsync(Guid id)
        {
            var filter = filterBuilder.Eq(e => e.Id, id);
            return await collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Item item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            await collection.InsertOneAsync(item);
        }

        public async Task UpdateAsync(Item item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            var filter = filterBuilder.Eq(e => e.Id, item.Id);
            await collection.ReplaceOneAsync(filter, item);
        }
        public async Task DeleteAsync(Guid id)
        {
            var filter = filterBuilder.Eq(e => e.Id, id);
            await collection.DeleteOneAsync(filter);
        }
    }
}