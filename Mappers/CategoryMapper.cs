using JoSale.DTO_s.CategoriesDTO_s;
using JoSale.Models;

namespace JoSale.Mappers
{
    public static class CategoryMapper
    {
        public static CategoryDTO ToCategoryDTO(this Category category)
        {
            return new CategoryDTO
            {
                Description = category.Description,
                Name = category.Name,
                Id = category.Id,
            };
        }
    }
}
