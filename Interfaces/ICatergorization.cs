using JoSale.DTO_s.CategoriesDTO_s;
using JoSale.Models;

namespace JoSale.Interfaces
{
    public interface ICatergorization
    {
        Task<CategorizeDTO> Categorize(Item Item);
    }
}
