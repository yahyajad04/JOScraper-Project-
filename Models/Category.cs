using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace JoSale.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<Item>? Items { get; set; } = new List<Item>();
        public float[]? Vector {  get; set; }
    }
}
