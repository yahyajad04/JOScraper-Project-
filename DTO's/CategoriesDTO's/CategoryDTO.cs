using System.ComponentModel.DataAnnotations;

namespace JoSale.DTO_s.CategoriesDTO_s
{
    public class CategoryDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
