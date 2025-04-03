using System;
using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Play.Common.Settings;

namespace Play.Common.RabbitMQ
{
    public static class Extensions
    {
        public static IServiceCollection AddRabbitMQ(this IServiceCollection services)
        {
            services.AddMassTransit(configure =>
            {
                configure.AddConsumers(Assembly.GetEntryAssembly());
                configure.AddConfigureEndpointsCallback((context, name, cfg) =>
                {
                    cfg.UseDelayedRedelivery(r => r.Intervals(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(30)));
                    cfg.UseMessageRetry(r => r.Immediate(5));
                });

                configure.UsingRabbitMq((seriveProvider, configurator) =>
                {
                    var config = seriveProvider.GetService<IConfiguration>();
                    var serviceSettings = config.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
                    var rabbitMQSettings = config.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>();

                    configurator.Host(rabbitMQSettings.Host);
                    configurator.ConfigureEndpoints(seriveProvider, new KebabCaseEndpointNameFormatter(serviceSettings.ServiceName, false));
                });
            });

            return services;

        }
    }
}