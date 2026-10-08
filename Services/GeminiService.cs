using JoSale.DTO_s.GeminiServiceDTO_s;
using JoSale.Interfaces.IServices;
using System.Text.Json;

namespace JoSale.Services
{
    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public GeminiService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<GeminiResponse> GetCategoryName(string productName)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            var prompt = $$"""
            You are a product categorization system.

            Determine the most appropriate general category for the product below. Make Sure to Choose the Best Match 
            Category While also a good Description for it.

            Rules:
            - Return exactly one category.
            - The category must be a short and general category name.
            - The category name should be in the generalist form and plural always.
            - Do not explain your answer.
            - Do not return the product name.
            - Do not return the brand or model.
            - Do not return multiple categories.
            - Always try to make the category consists of 1 part if possible.

            example: Product : Lenovo Intel Core i7 256GB SSD
                     category : Laptops
            
            -In the before example I dont want Gaming Laptops or Work Laptops only Laptops as the general Form.
            Return the result in exactly this JSON format, Dont Add Charachter extra:
            {
                "category": "category name",
                "description": "category Description"
            }

            if there is a category from the available Categories sent to you that matches well for the
            product choose it.

            Products and available categories:
            {{productName}}
            """;

            var request = new
            {
                contents = new[]
                {
            new
            {
                parts = new[]
                {
                    new
                    {
                        text = prompt
                    }
                }
            }
        }
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add(
                "x-goog-api-key",
                apiKey);

            var response = await _httpClient.PostAsJsonAsync(
                "https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash-lite:generateContent",
                request);

            var responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Status: {(int)response.StatusCode}");
            Console.WriteLine(responseBody);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(json);

            var text = document
                .RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            var result =
                JsonSerializer.Deserialize<GeminiResponse>(text!);

            return result!;
        }
    }
}
