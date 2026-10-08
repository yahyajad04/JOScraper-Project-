using JoSale.Interfaces.IServices;
using Microsoft.Extensions.AI;

namespace JoSale.Services
{
    public class EmbeddingService : IEmbedding<EmbeddingService>
    {
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
        public EmbeddingService(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
        {
            _embeddingGenerator = embeddingGenerator;
        }

        public async Task<float[]> GetEmbeddingVector(string text)
        {
            var embeddingText = $"task: classification | query: {text}";
            var vector = await _embeddingGenerator.GenerateVectorAsync(embeddingText);

            return vector.ToArray();
        }
    }
}
