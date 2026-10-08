using System.Text.Json.Serialization;

namespace JoSale.DTO_s.GeminiServiceDTO_s
{
    public class GeminiResponse
    {
        [JsonPropertyName("category")]
        public string? Category { get; set; }
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}
