using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Play.Catalog.Service.Entities;

namespace Play.Catalog.Service.Repositories
{    public class ItemsRepository : IRepository
    {
        private const string collectionName = "items";
        private readonly IMongoCollection<Item> dbCollection;
        /// <summary>
        /// /To build the filter to query for items
        /// </summary>
        private readonly FilterDefinitionBuilder<Item> filterBuilder = Builders<Item>.Filter;

        public ItemsRepository()
        {
            var mongoClient = new MongoClient("mongodb://localhost:27017");
            var database = mongoClient.GetDatabase("Catalog");
            dbCollection = database.GetCollection<Item>(collectionName);
        }

        public async Task<IReadOnlyCollection<Item>> GetAll()
        {
            return await (await dbCollection.FindAsync(filterBuilder.Empty)).ToListAsync();
        }
        public async Task<Item> Get(Guid id)
        {
            FilterDefinition<Item> filter = GetFilterById(id);
            return await (await dbCollection.FindAsync(filter)).FirstOrDefaultAsync();
        }
        public async Task Create(Item entity)
        {
            if(entity == null){
                throw new ArgumentNullException(nameof(entity));
            }
            await dbCollection.InsertOneAsync(entity);
        }
        public async Task Update(Item entity)
        {
            if(entity == null){
                throw new ArgumentNullException(nameof(entity));
            }

            FilterDefinition<Item> filter = GetFilterById(entity.Id);
            await dbCollection.ReplaceOneAsync(filter, entity);
        }

        public async Task Remove(Guid id)
        {
            var filter = GetFilterById(id);
            await dbCollection.DeleteOneAsync(filter);
        }

        private FilterDefinition<Item> GetFilterById(Guid id)
        {
            return filterBuilder.Eq(ent => ent.Id, id);
        }
    }
}