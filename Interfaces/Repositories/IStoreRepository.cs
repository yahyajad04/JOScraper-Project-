using JoSale.Models;

namespace JoSale.Interfaces.Repositories
{
    public interface IStoreRepository
    {
        Task<List<Store>> GetAll();
        Task<Store> GetById(int StoreId);
        Task<Store> GetByName(string Name);
        Task CreateStore(Store store);
    }
}
