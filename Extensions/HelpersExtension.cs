using Hangfire;
using JoSale.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel.Connectors.Google;
using Serilog;
using StackExchange.Redis;

namespace JoSale.Extensions
{
    public static class HelpersExtension
    {
        public static IServiceCollection AddHelperServices(this IServiceCollection services,
            IConfiguration configuration, IHostBuilder host)
        {
            //SQL Server Service
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(
                configuration.GetConnectionString("Conn")));

            //Hangfire Service for Background Services
            services.AddHangfire(config =>
                config.UseSqlServerStorage(
                    configuration.GetConnectionString("HangfireConnection")));

            services.AddHangfireServer();

            //Redis Caching Configuration
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer
                .Connect("localhost,abortConnect=false"));

            //Google Embedding/AI Service
            services.AddGoogleAIEmbeddingGenerator(
                modelId: "gemini-embedding-2",
                apiKey: configuration["Gemini:ApiKey"]!,
                apiVersion: GoogleAIVersion.V1
            );

            //Serilog Logger Configuration
            host.UseSerilog((hostingContext, loggerInformation) =>
            loggerInformation.ReadFrom.Configuration(hostingContext.Configuration));

            return services;
        }
    }
}
