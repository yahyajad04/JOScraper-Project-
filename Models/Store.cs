using System.ComponentModel.DataAnnotations;

namespace JoSale.Models
{
    public class Store
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string URL { get; set; }
        public string? ApiURL { get; set; }
        public string? About {  get; set; }
    }
}
