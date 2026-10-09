using JoSale.Interfaces;
using JoSale.Interfaces.IServices;
using JoSale.Repositories;
using JoSale.Services;

namespace JoSale.Extensions
{
    public static class SeriviceExtension
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            //Store Scrapings
            services.AddScoped<IStoreScraper<OrientalStoreScraper>, OrientalStoreScraper>();
            services.AddScoped<IStoreScraper<CityCenterScraper>, CityCenterScraper>();
            services.AddScoped<IStoreScraper<MidasStore>, MidasStore>();

            //Services
            services.AddScoped<ICatergorization, Categorization>();
            services.AddScoped<IEmbedding<EmbeddingService>, EmbeddingService>();
            services.AddScoped<IEmbedding<JinaEmbedding>, JinaEmbedding>();
            services.AddScoped<IGeminiService, GeminiService>();
            services.AddScoped<ISemanticSearch, SemanticSearchService>();
            services.AddScoped<IScrapingJob, ScrapingJobService>();
            services.AddScoped<ICacheService, CacheService>();

            return services;
        }
    }
}
