using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Play.Common.Settings;

namespace Play.Common.MongoDB
{
    public static class Extensions
    {
        public static IServiceCollection AddMongo(this IServiceCollection services)
        {
            //whenever we store a doc in mongo with guid, it'll be kept as a string
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
            //whenever we store a doc in mongo with dateTimeOffset, it'll be kept as a string
            BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));            
            //creating the client before registering in the container
            services.AddSingleton(serviceProvider => {
                var config = serviceProvider.GetService<IConfiguration>();
                var settings = config.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
                var mongoDbSettings = config.GetSection(nameof(MongoDBSettings)).Get<MongoDBSettings>();
                var mongoClient = new MongoClient(mongoDbSettings.ConnectionString);
                return mongoClient.GetDatabase(settings.ServiceName);
            });
            return services;
        }

        public static IServiceCollection AddMongoRepository<T>(this IServiceCollection services, string collectionName) 
            where T : IEntity
        {
            //he did it after all
            services.AddSingleton<IRepository<T>>(serviceProvider => {
                //Here you can access any previous service provider that's already been registered
                var database = serviceProvider.GetService<IMongoDatabase>();
                return new MongoRepository<T>(database, collectionName);
            });
            return services;
        }
    }
}