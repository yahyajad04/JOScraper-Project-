using JoSale.DTO_s.ItemsDTO_s;
using JoSale.Models;

namespace JoSale.Mappers
{
    public static class ItemMapper
    {
        public static ItemDTO ToItemDTO(this Item item)
        {
            return new ItemDTO
            {
                Name = item.Name,
                Description = item.Description,
                Category = item.Category?.ToCategoryDTO(),
                CategoryId = item.CategoryId
            };
        }
    }
}
