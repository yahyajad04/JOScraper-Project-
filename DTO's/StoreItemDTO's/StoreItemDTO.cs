using JoSale.DTO_s.ItemsDTO_s;
using JoSale.DTO_s.StoreDTO_s;
using JoSale.Models;

namespace JoSale.DTO_s.StoreItemDTO_s
{
    public class StoreItemDTO
    {
        public int StoreId { get; set; }
        public StoreDTO? Store { get; set; }
        public int ItemId { get; set; }
        public ItemDTO? Item { get; set; }
        public decimal Price { get; set; }
        public string? ProductUrl { get; set; }
        public string? ImageURL { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
