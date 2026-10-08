using JoSale.Data;
using JoSale.Interfaces.Repositories;
using JoSale.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace JoSale.Repositories
{
    public class StoreRepository : IStoreRepository
    {
        private readonly AppDbContext _context;

        public StoreRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateStore(Store store)
        {
            _context.Stores.Add(store);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Store>> GetAll()
        {
            return await _context.Stores.ToListAsync();
        }
        public async Task<Store> GetById(int StoreId)
        {
            var store = await _context.Stores.FirstOrDefaultAsync(s => s.Id == StoreId);
            if (store == null)
                throw new Exception("Store Doesnt Exist");

            return store;
        }

        public async Task<Store> GetByName(string Name)
        {
            return await _context.Stores.FirstOrDefaultAsync(n => n.Name.Equals(Name));
        }
    }
}
