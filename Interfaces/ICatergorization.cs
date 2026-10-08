using JoSale.Models;

namespace JoSale.Interfaces
{
    public interface ICatergorization
    {
        Task<Category> Categorize(Item Item);
    }
}
