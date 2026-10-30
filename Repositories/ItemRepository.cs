using JoSale.Data;
using JoSale.Interfaces.IServices;
using JoSale.Interfaces.Repositories;
using JoSale.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Runtime.CompilerServices;

namespace JoSale.Repositories
{
    public class ItemRepository : IItemRepository
    {
        private readonly AppDbContext _context;
        private readonly ISemanticSearch _semantic;
        public ItemRepository(AppDbContext context, ISemanticSearch semantic)
        {
            _context = context;
            _semantic = semantic;
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

        public async Task<Dictionary<Item, Item>> CreateItems(List<Item> items)
        {
            var dict = new Dictionary<Item, Item>();

            var exist = await _context.Items.ToListAsync();
            var existNames = exist.Select(e => e.NormalizedName);

            var newItems = items.DistinctBy(it => it.NormalizedName)
                    .Where(it => !existNames.Contains(it.NormalizedName))
                    .ToList();

            float maxSim = 0;
            var it = new Item();

            if (!exist.IsNullOrEmpty())
            {
                foreach (var item in newItems)
                {
                    maxSim = 0;
                    if (item.Vector != null && item.Vector.Length > 2)
                    {
                        foreach (var item2 in exist)
                        {
                            if (item2.Vector != null && item2.Vector.Length > 2)
                            {
                                var sim = _semantic.Similarity(item.Vector, item2.Vector);

                                if (maxSim < sim)
                                {
                                    maxSim = sim;
                                    it = item2;
                                }
                            }
                        }
                        if(maxSim > 0.92)
                        {
                            Console.WriteLine($"++ SIMIARITY : {maxSim}\n" +
                             $"ITEM NAME: {item.Name}\nExist Item Name: {it?.Name}\n\n");
                            dict[item] = it;
                        }
                    }
                }
            }

            newItems.RemoveAll(item => dict.ContainsKey(item));

            _context.Items.AddRange(newItems);
            await _context.SaveChangesAsync();

            foreach (var item in newItems)
            {
                if(!dict.ContainsKey(item))
                    dict[item] = item;
            }
            return dict;
        }
    }
}
