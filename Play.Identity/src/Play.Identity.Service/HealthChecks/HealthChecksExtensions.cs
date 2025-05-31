using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;
using Play.Common.Settings;

namespace Play.Identity.Service.HealthChecks
{
    public static class HealthChecksExtensions
    {
        private const string MongoCheckName = "mongoDb";
        private const string ReadyTagName = "ready";
        private static readonly TimeSpan DefaultSeconds = TimeSpan.FromSeconds(3);
        public static IHealthChecksBuilder AddMongoDbHealthCheck(
            this IHealthChecksBuilder builder,
            TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
                    MongoCheckName,
                    serviceProvider =>
                    {
                        var configuration = serviceProvider.GetService<IConfiguration>();
                        var mongoDbSettings = configuration.GetSection(nameof(MongoDbSettings))
                                                            .Get<MongoDbSettings>();
                        var mongoClient = new MongoClient(mongoDbSettings.ConnectionString);
                        return new MongoConnectionHealthChecks(mongoClient);
                    },
                    HealthStatus.Unhealthy,
                    new[] { ReadyTagName },
                    timeout ?? DefaultSeconds
                ));
        }
    }
}