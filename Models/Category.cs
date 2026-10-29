using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace JoSale.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; }
        public string? Description { get; set; }
        [JsonIgnore]
        public List<Item>? Items { get; set; } = new List<Item>();
        public float[]? Vector {  get; set; }
    }
}
