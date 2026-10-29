using JoSale.Data;
using JoSale.Helpers;
using JoSale.Interfaces.IServices;
using JoSale.Interfaces.Repositories;
using JoSale.Models;
using Microsoft.EntityFrameworkCore;

namespace JoSale.Repositories
{
    public class StoreItemRepository : IStoreItem
    {
        private readonly AppDbContext _context;
        private readonly ISemanticSearch _semantic;
        public StoreItemRepository(AppDbContext context, ISemanticSearch semantic)
        {
            _context = context;
            _semantic = semantic;
        }
        public async Task CreateStoreItem(StoreItem item)
        {
            _context.StoreItem.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task CreateStoreItems(List<StoreItem> storeItems)
        {
            //This is to get the current key pairs we have in the database
            var existKeys = (await _context.StoreItem
                .Select(s => new { s.ItemId, s.StoreId })
                .ToListAsync())
                .Select(s => (s.ItemId, s.StoreId))
                .ToHashSet();

            //This is to prevent duplicates from the current scrape and from the database too
            var extract = storeItems
                .Where(s => s.ItemId != 0)
                .DistinctBy(s => (s.ItemId, s.StoreId))
                .Where(s => existKeys.Add((s.ItemId, s.StoreId)))
                .ToList();

            if (extract.Count == 0)
                return;

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
                .Include(i => i.Item)
                .ThenInclude(c => c.Category)
                .AsQueryable();

            var skipnum = (query.PageNumber - 1) * query.PageSize;

            return await storeItems.Skip(skipnum).Take(query.PageSize).ToListAsync();
        }


        //This is used by the Scrapers functions, Dont Remove it!
        public async Task<List<StoreItem>> GetAllFromStore(int StoreId)
        {
            return await _context.StoreItem
                .AsNoTracking()
                .Where(s => s.StoreId == StoreId)
                .Include(i => i.Item).ToListAsync();
        }
        public async Task<List<StoreItem>> GetAllByItemId(int ItemId)
        {
            return await _context.StoreItem.Where(i => i.ItemId == ItemId).ToListAsync();
        }
    }
}
