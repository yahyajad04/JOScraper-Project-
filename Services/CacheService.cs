using JoSale.Interfaces.IServices;
using StackExchange.Redis;
using System.Text.Json;

namespace JoSale.Services
{
    public class CacheService : ICacheService
    {
        private readonly IDatabase _redis;
        public CacheService(IConnectionMultiplexer connection)
        {
            _redis = connection.GetDatabase();
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _redis.StringGetAsync(key);

            if (!value.HasValue)
                return default;

            return JsonSerializer.Deserialize<T>(value!);
        }

        public async Task RemoveAsync(string key)
        {
            await _redis.KeyDeleteAsync(key);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            var json = JsonSerializer.Serialize(value);

            await _redis.StringSetAsync(key, json, expiration);
        }
    }
}
