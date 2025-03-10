
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
        public static IServiceCollection AddMongo(this IServiceCollection services){
        
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
            BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));


            services.AddSingleton(seriveProvider => {
                var config = seriveProvider.GetService<IConfiguration>();

                var mongoDbSettings = config.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
                var serviceSettings = config.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
                var mongoClient = new MongoClient(mongoDbSettings.ConnectionString);
                return mongoClient.GetDatabase(serviceSettings.ServiceName);
            });
            return services;
        }

        public static IServiceCollection AddMongoRepository<T>(this IServiceCollection services, string collectionName) 
        where T : IEntity {
            services.AddSingleton<IRepository<T>>(seriveProvider => {
                var db = seriveProvider.GetService<IMongoDatabase>();
                return new MongoRepository<T>(db, collectionName);
            });
            return services;
        }
    }
}