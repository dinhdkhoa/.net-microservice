using System;
using System.Reflection;
using GreenPipes;
using GreenPipes.Configurators;
using MassTransit;
using MassTransit.Definition;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Play.Common.Settings;

namespace Play.Common.RabbitMQ
{
    public static class Extensions
    {
        public static IServiceCollection AddRabbitMQ(this IServiceCollection services, Action<IRetryConfigurator> configRetries = null)
        {
            services.AddMassTransit(configure =>
            {
                configure.AddConsumers(Assembly.GetEntryAssembly());
                configure.UsePlayEconomyRabbitMQ(configRetries);
            });
            services.AddMassTransitHostedService();
            return services;

        }

        public static void UsePlayEconomyRabbitMQ(this IBusRegistrationConfigurator configure, Action<IRetryConfigurator> configRetries = null)
        {
            configure.AddConfigureEndpointsCallback((queueName, cfg) =>
            {
                // cfg.UseDelayedRedelivery(r => r.Intervals(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(30)));
                if(configRetries == null){
                    configRetries = (retryConfigurator) => retryConfigurator.Interval(3,TimeSpan.FromSeconds(5));
                }
                cfg.UseMessageRetry(configRetries);
            });

            configure.UsingRabbitMq((seriveProvider, configurator) =>
            {
                var config = seriveProvider.GetService<IConfiguration>();
                var serviceSettings = config.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
                var rabbitMQSettings = config.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>();

                configurator.Host(rabbitMQSettings.Host);
                configurator.ConfigureEndpoints(seriveProvider, new KebabCaseEndpointNameFormatter(serviceSettings.ServiceName, false));
            });
        }
    }
}