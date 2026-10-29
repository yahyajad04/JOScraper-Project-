using JoSale.DTO_s.StoreItemDTO_s;
using JoSale.Models;

namespace JoSale.Mappers
{
    public static class StoreItemMapper
    {
        public static StoreItemDTO toStoreItemDTO(this StoreItem storeItem)
        {
            return new StoreItemDTO
            {
                Item = storeItem.Item?.ToItemDTO(),
                ImageURL = storeItem.ImageURL,
                ItemId = storeItem.ItemId,
                LastUpdated = storeItem.LastUpdated,
                Price = storeItem.Price,
                ProductUrl = storeItem.ProductUrl,
                Store = storeItem.Store?.ToStoreDTO(),
                StoreId = storeItem.StoreId,   
            };
        }
    }
}
