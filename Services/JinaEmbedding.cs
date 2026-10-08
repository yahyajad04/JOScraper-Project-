using JoSale.DTO_s.JinaServiceDTO_s;
using JoSale.Interfaces.IServices;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace JoSale.Services
{
    public class JinaEmbedding : IEmbedding<JinaEmbedding>
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public JinaEmbedding(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<float[]> GetEmbeddingVector(string text)
        {
            var apiKey = _configuration["Jina:ApiKey"];

            var requestBody = new
            {
                model = "jina-embeddings-v5-text-small",
                task = "text-matching",
                normalized = true,
                input = new[]
                {
                    text
                }
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var response = await _httpClient.PostAsync("https://api.jina.ai/v1/embeddings", content);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<JinaResponseDTO>(responseBody);

            return result!.Data[0].Embedding.ToArray();

        }
    }
}
