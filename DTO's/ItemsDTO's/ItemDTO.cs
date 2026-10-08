using JoSale.Models;
using System.ComponentModel.DataAnnotations;

namespace JoSale.DTO_s.ItemsDTO_s
{
    public class ItemDTO
    {
        [Required]
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public string? Description { get; set; }
    }
}
