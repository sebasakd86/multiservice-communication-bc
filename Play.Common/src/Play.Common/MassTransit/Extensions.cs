using System;
using System.Reflection;
using GreenPipes;
using MassTransit;
using MassTransit.Definition;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Play.Common.Settings;

namespace Play.Common.MassTransit
{
    public static class Extensions
    {
        public static IServiceCollection AddMassTransitWithRabbitMq(this IServiceCollection services)
        {
            services.AddMassTransit(configure => 
            {
                configure.AddConsumers(Assembly.GetEntryAssembly()); // any consumer class

                configure.UsingRabbitMq((context, configurator) => 
                {
                    var config = context.GetService<IConfiguration>();
                    var settings = config.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
                    var rabbitMqSettings = config.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>();
                    configurator.Host(rabbitMqSettings.Host);
                    configurator.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(settings.ServiceName, false));
                    configurator.UseMessageRetry(retryConfig => 
                    {
                        //how many times to retry the method consumption evey X time inbetween.
                        retryConfig.Interval(3, TimeSpan.FromSeconds(5));
                    });
                });
            });
            //Starts the RabbitMQ bus
            services.AddMassTransitHostedService();

            return services;
        }
    }
}