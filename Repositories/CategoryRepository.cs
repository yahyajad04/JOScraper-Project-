using HtmlAgilityPack;
using JoSale.Data;
using JoSale.DTO_s.CategoriesDTO_s;
using JoSale.Interfaces.Repositories;
using JoSale.Models;
using Microsoft.EntityFrameworkCore;

namespace JoSale.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;
        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Category> CreateCategory(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return category;
        }

        public async Task<List<Category>> GetAll()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task<Category> GetCategoryByName(string name)
        {
            return await _context.Categories.FirstOrDefaultAsync(c => c.Name == name);
        }

        public async Task UpdateCategory(UpdateCategoryDTO updateDTO)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id ==  updateDTO.Id);

            if (category == null)
                throw new Exception("Category Not Found");

            category.Name = updateDTO.Name ?? category.Name;
            category.Description = updateDTO.Description ?? category.Description;
            category.Vector = updateDTO.Vector ?? category.Vector;

            await _context.SaveChangesAsync();
        }
    }
}
