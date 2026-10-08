using JoSale.Models;

namespace JoSale.Interfaces.IServices
{
    public interface IEmbedding<T>
    {
        Task<float[]> GetEmbeddingVector(string text);
    }
}
