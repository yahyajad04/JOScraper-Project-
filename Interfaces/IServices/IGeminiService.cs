using JoSale.DTO_s.GeminiServiceDTO_s;

namespace JoSale.Interfaces.IServices
{
    public interface IGeminiService
    {
        Task<GeminiResponse> GetCategoryName(string prompt);
    }
}
