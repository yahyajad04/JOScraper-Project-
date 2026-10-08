using JoSale.Models;

namespace JoSale.Interfaces.Repositories
{
    public interface IItemRepository
    {
        Task<List<Item>> GetAll();
        Task CreateItem(Item item);
        Task<List<Item>> CreateItems(List<Item> items);
    }
}
