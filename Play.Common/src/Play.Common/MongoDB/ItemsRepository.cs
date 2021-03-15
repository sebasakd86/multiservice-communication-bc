using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Play.Common.MongoDB
{
    public class MongoRepository<T> : IRepository<T> where T : IEntity
    {
        private readonly IMongoCollection<T> dbCollection;
        /// <summary>
        /// /To build the filter to query for items
        /// </summary>
        private readonly FilterDefinitionBuilder<T> filterBuilder = Builders<T>.Filter;
        private readonly string _collectionName;

        public MongoRepository(IMongoDatabase database, string collectionName)
        {
            this._collectionName = collectionName;
            dbCollection = database.GetCollection<T>(collectionName);
        }

        public async Task<IReadOnlyCollection<T>> GetAll()
        {
            return await (await dbCollection.FindAsync(filterBuilder.Empty)).ToListAsync();
        }
        public async Task<IReadOnlyCollection<T>> GetAll(Expression<Func<T, bool>> filter)
        {
            return await (await dbCollection.FindAsync(filter)).ToListAsync();
        }
        public async Task<T> Get(Guid id)
        {
            FilterDefinition<T> filter = GetFilterById(id);
            return await (await dbCollection.FindAsync(filter)).FirstOrDefaultAsync();
        }
        public async Task<T> Get(Expression<Func<T, bool>> filter)
        {
            return await (await dbCollection.FindAsync(filter)).FirstOrDefaultAsync();
        }
        public async Task Create(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            await dbCollection.InsertOneAsync(entity);
        }
        public async Task Update(Guid id, T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            FilterDefinition<T> filter = GetFilterById(id);
            await dbCollection.ReplaceOneAsync(filter, entity);
        }

        public async Task Remove(Guid id)
        {
            var filter = GetFilterById(id);
            await dbCollection.DeleteOneAsync(filter);
        }

        private FilterDefinition<T> GetFilterById(Guid id)
        {
            return filterBuilder.Eq(ent => ent.Id, id);
        }
    }
}