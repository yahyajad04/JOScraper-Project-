using JoSale.DTO_s.ItemsDTO_s;
using JoSale.Models;

namespace JoSale.Interfaces
{
    public interface IStoreScraper<T>
    {
        Task<List<StoreItem>> ScrapeStore();
        Task<List<ScrapedItem>> CrawlStore(Store store);
    }
}
