using JoSale.Data;
using JoSale.Interfaces.Repositories;
using JoSale.Models;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace JoSale.Repositories
{
    public class ItemRepository : IItemRepository
    {
        private readonly AppDbContext _context;
        public ItemRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Item>> GetAll()
        {
            return await _context.Items.ToListAsync();
        }

        public async Task CreateItem(Item item)
        {
            _context.Items.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Item>> CreateItems(List<Item> items)
        {
            var existNames = await _context.Items.Select(e => e.Name).ToListAsync();

            var newItems = items.DistinctBy(it => it.Name)
                    .Where(it => !existNames.Contains(it.Name))
                    .ToList();

            _context.Items.AddRange(newItems);
            await _context.SaveChangesAsync();

            return newItems;
        }
    }
}
