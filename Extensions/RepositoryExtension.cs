using JoSale.Interfaces.Repositories;
using JoSale.Repositories;

namespace JoSale.Extensions
{
    public static class RepositoryExtension 
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            //repositories
            services.AddScoped<IStoreRepository, StoreRepository>();
            services.AddScoped<IStoreItem, StoreItemRepository>();
            services.AddScoped<IItemRepository, ItemRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();

            return services;
        }
    }
}
