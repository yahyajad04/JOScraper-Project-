using JoSale.Models;

namespace JoSale.DTO_s.ItemsDTO_s
{
    public class ScrapedItem
    {
        public Item Item { get; set; }
        public decimal Price { get; set; }
        public string? ProductURL { get; set; }
        public string? ImageURL { get; set; }
    }
}
