using JoSale.DTO_s.StoreDTO_s;
using JoSale.Models;

namespace JoSale.Mappers
{
    public static class StoreMapper
    {
        public static StoreDTO ToStoreDTO(this Store store)
        {
            return new StoreDTO
            {
                About = store.About,
                ApiURL = store.ApiURL,
                Id = store.Id,
                Name = store.Name,
                URL = store.URL
            };
        }
        public static Store FromCreateDTO(this CreateStoreDTO createStoreDTO)
        {
            return new Store
            {
                Name = createStoreDTO.Name,
                URL = createStoreDTO.URL,
                About = createStoreDTO?.About,
                ApiURL = createStoreDTO?.ApiURL,
            };
        } 
    }
}
