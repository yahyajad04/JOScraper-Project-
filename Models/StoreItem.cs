using System.ComponentModel.DataAnnotations;

namespace JoSale.Models
{
    public class StoreItem
    {
        public int StoreId { get; set; }
        public Store? Store { get; set; }
        public int ItemId { get; set; }
        public Item? Item { get; set; }
        public decimal Price { get; set; }
        public string? ProductUrl { get; set; }
        public string? ImageURL { get; set; }
        public DateTime LastUpdated { get; set; }

    }
}
