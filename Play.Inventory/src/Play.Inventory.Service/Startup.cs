using System;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Play.Common.MassTransit;
using Play.Common.MongoDB;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Entities;
using Polly;
using Polly.Timeout;

namespace Play.Inventory.Service
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddMongo()
                    .AddMongoRepository<InventoryItem>("inventoryItems")
                    .AddMongoRepository<CatalogItem>("catalogItems")
                    .AddMassTransitWithRabbitMq();

            AddCatalogClient(services);

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Play.Inventory.Service", Version = "v1" });
            });
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Play.Inventory.Service v1"));
            }

            // app.UseHttpsRedirection();

            app.UseRouting();

            // app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
private static void AddCatalogClient(IServiceCollection services)
        {
            Random jitterer = new Random();

            services.AddHttpClient<CatalogClient>(client =>
            {
                client.BaseAddress = new System.Uri("http://localhost:5000");
            })
            .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().WaitAndRetryAsync(
                5, //ammount of retries
                   //time it takes to retry, in this case, exponential wait
                retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitterer.Next(1000)) //2-4-8-16-32
                // onRetry: (outcome, timeSpan, retryAttempt) => 
                // {
                //     var serviceProvider = services.BuildServiceProvider();
                //     serviceProvider.GetService<ILogger<CatalogClient>>()?
                //         .LogWarning($"Delaying for {timeSpan.TotalSeconds}, then making retry {retryAttempt}");
                // }
            ))
            .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().CircuitBreakerAsync(
                3, //until the circuit breaks
                TimeSpan.FromSeconds(15) //time until it re-enables the circuit after 3 fails
            ))
            .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));
        }        
    }
}
