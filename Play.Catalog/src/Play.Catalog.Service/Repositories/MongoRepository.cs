using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Play.Catalog.Service.Entities;

namespace Play.Catalog.Service.Repositories
{

    public class MongoRepository<T> :  IRepository<T> where T : IEntity
    {
        private readonly IMongoCollection<T> collection;
        private readonly FilterDefinitionBuilder<T> filterBuilder = Builders<T>.Filter;

        public MongoRepository(IMongoDatabase db, string collectionName)
        {
            // var mongoClient = new MongoClient("mongodb://localhost:27017");
            // var db = mongoClient.GetDatabase("Catalog");
            collection = db.GetCollection<T>(collectionName);

        }

        public async Task<List<T>> GetListAsync()
        {
            return await collection.Find(filterBuilder.Empty).ToListAsync();
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            var filter = filterBuilder.Eq(e => e.Id, id);
            return await collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            await collection.InsertOneAsync(entity);
        }

        public async Task UpdateAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            var filter = filterBuilder.Eq(e => e.Id, entity.Id);
            await collection.ReplaceOneAsync(filter, entity);
        }
        public async Task DeleteAsync(Guid id)
        {
            var filter = filterBuilder.Eq(e => e.Id, id);
            await collection.DeleteOneAsync(filter);
        }
    }
}