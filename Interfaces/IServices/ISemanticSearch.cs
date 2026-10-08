namespace JoSale.Interfaces.IServices
{
    public interface ISemanticSearch
    {
        float Similarity(float[] vector1, float[] vector2);
    }
}
