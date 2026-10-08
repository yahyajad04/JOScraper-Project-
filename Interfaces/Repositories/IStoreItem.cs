using JoSale.Helpers;
using JoSale.Models;

namespace JoSale.Interfaces.Repositories
{
    public interface IStoreItem
    {
        Task<List<StoreItem>> GetAll(QueryObject query);
        Task<List<StoreItem>> GetAllFromStore(int StoreId, QueryObject query);
        Task<List<StoreItem>> GetAllFromStore(int StoreId);
        Task<List<StoreItem>> GetAllByItemId(int ItemId);
        Task CreateStoreItem(StoreItem item);
        Task<StoreItem> UpdatePrice(int StoreId, int ItemId, decimal price);
        Task CreateStoreItems(List<StoreItem> items);
    }
}
