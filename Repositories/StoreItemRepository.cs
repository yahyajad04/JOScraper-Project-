using JoSale.Data;
using JoSale.Helpers;
using JoSale.Interfaces.Repositories;
using JoSale.Models;
using Microsoft.EntityFrameworkCore;

namespace JoSale.Repositories
{
    public class StoreItemRepository : IStoreItem
    {
        private readonly AppDbContext _context;
        public StoreItemRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task CreateStoreItem(StoreItem item)
        {
            _context.StoreItem.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task CreateStoreItems(List<StoreItem> storeItems)
        {
            var extract = storeItems.DistinctBy(c => new { c.Item.Name, c.StoreId }).ToList(); 

            _context.StoreItem.AddRange(extract);
            await _context.SaveChangesAsync();
        }

        public async Task<StoreItem> UpdatePrice(int StoreId ,int ItemId, decimal price)
        {
            var storeItem = await _context.StoreItem.FirstOrDefaultAsync(i => i.StoreId == StoreId && i.ItemId == ItemId);

            if (storeItem == null)
                throw new Exception("This Store Item Doesnt Exist");

            storeItem.Price = price;
            await _context.SaveChangesAsync();
            return storeItem;
        }
        public async Task<List<StoreItem>> GetAll(QueryObject query)
        {
            var storeItems = _context.StoreItem.Include(i => i.Item).AsQueryable();
            var skipnum = (query.PageNumber - 1) * query.PageSize;

            return await storeItems.Skip(skipnum).Take(query.PageSize).ToListAsync();
        }

        public async Task<List<StoreItem>> GetAllFromStore(int StoreId, QueryObject query)
        {
            var storeItems = _context.StoreItem
                .Where(s => s.StoreId == StoreId)
                .Include(i => i.Item).AsQueryable();

            var skipnum = (query.PageNumber - 1) * query.PageSize;

            return await storeItems.Skip(skipnum).Take(query.PageSize).ToListAsync();
        }
        public async Task<List<StoreItem>> GetAllFromStore(int StoreId)
        {
            return await _context.StoreItem
                .Where(s => s.StoreId == StoreId)
                .Include(i => i.Item).ToListAsync();
        }
        public async Task<List<StoreItem>> GetAllByItemId(int ItemId)
        {
            return await _context.StoreItem.Where(i => i.ItemId == ItemId).ToListAsync();
        }
    }
}
