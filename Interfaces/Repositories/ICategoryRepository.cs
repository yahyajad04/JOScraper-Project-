using JoSale.DTO_s.CategoriesDTO_s;
using JoSale.Models;

namespace JoSale.Interfaces.Repositories
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetAll();
        Task UpdateCategory(UpdateCategoryDTO updateDTO);
        Task<Category> CreateCategory(Category category);
        Task<Category> GetCategoryByName(string name);
    }
}
