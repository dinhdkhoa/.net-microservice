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
        private const string RabbitMQ = "RABBIT_MQ";
        private const string ServiceBus = "SERVICE_BUS";
        public static IServiceCollection AddMessageBroker(this IServiceCollection services, IConfiguration config, Action<IRetryConfigurator> configRetries = null)
        {
            var serviceSettings = config.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
            switch (serviceSettings.MessageBroker?.ToUpper())
            {
                case ServiceBus:
                    services.AddAzureServiceBus(configRetries);
                    break;
                case RabbitMQ:
                default:
                    services.AddRabbitMQ(configRetries);
                    break;
            }
            return services;
        }
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
        public static IServiceCollection AddAzureServiceBus(this IServiceCollection services, Action<IRetryConfigurator> configRetries = null)
        {
            services.AddMassTransit(configure =>
            {
                configure.AddConsumers(Assembly.GetEntryAssembly());
                configure.UsePlayEconomyAzureServiceBus(configRetries);
            });
            services.AddMassTransitHostedService();
            return services;
        }

        public static void UsePlayEconomyRabbitMQ(this IBusRegistrationConfigurator configure, Action<IRetryConfigurator> configRetries = null)
        {
            configure.AddConfigureEndpointsCallback((queueName, cfg) =>
            {
                // cfg.UseDelayedRedelivery(r => r.Intervals(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(30)));
                if (configRetries == null)
                {
                    configRetries = (retryConfigurator) => retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
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
        public static void UsePlayEconomyAzureServiceBus(this IBusRegistrationConfigurator configure, Action<IRetryConfigurator> configRetries = null)
        {
            configure.AddConfigureEndpointsCallback((queueName, cfg) =>
            {
                // cfg.UseDelayedRedelivery(r => r.Intervals(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(30)));
                if (configRetries == null)
                {
                    configRetries = (retryConfigurator) => retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
                }
                cfg.UseMessageRetry(configRetries);
            });

            configure.UsingAzureServiceBus((seriveProvider, configurator) =>
            {
                var config = seriveProvider.GetService<IConfiguration>();
                var serviceSettings = config.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
                var serviceBusSettings = config.GetSection(nameof(ServiceBusSettings)).Get<ServiceBusSettings>();

                configurator.Host(serviceBusSettings.ConnectionString);
                configurator.ConfigureEndpoints(seriveProvider, new KebabCaseEndpointNameFormatter(serviceSettings.ServiceName, false));
            });
        }
    }
}