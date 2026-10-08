using System.ComponentModel.DataAnnotations;

namespace JoSale.Models
{
    public class Item
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string? NormalizedName { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public string? Description { get; set; }
    }
}
