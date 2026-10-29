using JoSale.DTO_s.CategoriesDTO_s;
using JoSale.Interfaces;
using JoSale.Interfaces.IServices;
using JoSale.Interfaces.Repositories;
using JoSale.Models;
using JoSale.Services;
using Microsoft.IdentityModel.Tokens;

namespace JoSale.Repositories
{
    public class Categorization : ICatergorization
    {
        private readonly IEmbedding<JinaEmbedding> _embedding;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ISemanticSearch _semanticSearch;
        private readonly IGeminiService _geminiService;
        public Categorization(IEmbedding<JinaEmbedding> embedding, ICategoryRepository categoryRepository,
            ISemanticSearch semanticSearch, IGeminiService geminiService)
        {
            _embedding = embedding;
            _categoryRepository = categoryRepository;
            _semanticSearch = semanticSearch;
            _geminiService = geminiService;
        }
        public async Task<CategorizeDTO> Categorize(Item Item)
        {
            var categories = await _categoryRepository.GetAll();
            var vector = new float[1];
            var categoryNames = "Categories: ";
            float maxSimilarity = -1;
            var finalCategory = categories[0];
            try
            {
                vector = await _embedding.GetEmbeddingVector(Item.Name);
            }
            catch (Exception ex)
            {
                var cat = new CategorizeDTO
                {
                    Category = finalCategory,
                    Vector = new float[1],
                };

                return cat;
            }


            Console.WriteLine($"Item Name: {Item.Name}");

            if(!categories.IsNullOrEmpty())
            {
                foreach(var category in categories)
                {
                    if(category.Id != categories[0].Id)
                        categoryNames += category.Name + ",";

                    if (category.Vector != null)
                    {
                        var sim = _semanticSearch.Similarity(category.Vector, vector);

                        Console.WriteLine($"Category: {category.Name} => Similarity: {sim}");

                        if (maxSimilarity < sim)
                        {
                            maxSimilarity = sim;
                            finalCategory = category;
                        }
                            
                    }
                    else
                    {
                        var vect2 = new float[1];
                        try
                        {
                            var catText = $"{category.Name}. {category.Description}";
                            vect2 = await _embedding.GetEmbeddingVector(catText);
                        }
                        catch(Exception ex)
                        {
                            var cat = new CategorizeDTO
                            {
                                Category = finalCategory,
                                Vector = new float[1]
                            };

                            return cat;
                        }
                        var sim = _semanticSearch.Similarity(vect2, vector);  
                        if (maxSimilarity < sim)
                        {
                            maxSimilarity = sim;
                            finalCategory = category;
                        }
                            
                        var updateDTO = new UpdateCategoryDTO
                        {
                            Id = category.Id,
                            Vector = vect2
                        };
                        await _categoryRepository.UpdateCategory(updateDTO);
                    }
                }
            }

            Console.WriteLine("++MAX SIMILARITY: " + maxSimilarity);
                
            if(maxSimilarity < 0.5)
            {  
                try
                {
                    categoryNames += "End of Categories. \nProduct Name: " + Item.Name;
                    var geminiRes = await _geminiService.GetCategoryName(categoryNames);
                    finalCategory = new Category
                    {
                        Name = geminiRes.Category,
                        Description = geminiRes.Description,
                    };

                    var exist = await _categoryRepository.GetCategoryByName(geminiRes.Category);

                    if (exist != null)
                    {
                        var cat = new CategorizeDTO
                        {
                            Category = exist,
                            Vector = vector
                        };
                        return cat;
                    }
                    else
                    {
                        var catText = $"{geminiRes.Category}. {geminiRes.Description}";
                        var categoryVector = await _embedding.GetEmbeddingVector(catText);
                        finalCategory.Vector = categoryVector;

                        finalCategory = await _categoryRepository.CreateCategory(finalCategory);

                        var cat = new CategorizeDTO
                        {
                            Category = finalCategory,
                            Vector = vector
                        };
                        return cat;
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                    var cat = new CategorizeDTO
                    {
                        Category = finalCategory,
                        Vector = vector
                    };
                    return cat;
                }
            }
            else
            {
                var cat = new CategorizeDTO
                {
                    Category = finalCategory,
                    Vector = vector
                };
                return cat;
            }
        }
    }
}
