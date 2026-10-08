using System.ComponentModel.DataAnnotations;

namespace JoSale.DTO_s.StoreDTO_s
{
    public class CreateStoreDTO
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string URL { get; set; }
        public string? ApiURL { get; set; }
        public string? About { get; set; }
    }
}
