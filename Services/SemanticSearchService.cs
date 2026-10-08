using JoSale.Interfaces.IServices;
using System.Numerics.Tensors;

namespace JoSale.Services
{
    public class SemanticSearchService : ISemanticSearch
    {
        public float Similarity(float[] vector1, float[] vector2)
        {
            return TensorPrimitives.CosineSimilarity(vector1, vector2);
        }
    }
}
