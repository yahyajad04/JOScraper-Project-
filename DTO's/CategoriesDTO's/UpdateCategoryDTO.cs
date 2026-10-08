namespace JoSale.DTO_s.CategoriesDTO_s
{
    public class UpdateCategoryDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public float[]? Vector {  get; set; }
    }
}
