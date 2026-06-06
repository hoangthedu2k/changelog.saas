using System.Text.Json;
using ChangelogSaas.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace ChangelogSaas.Infrastructure.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;

        public RedisCacheService(IDistributedCache cache) => _cache = cache;

        public async Task<T?> GetAsync<T>(string key)
        {
            var json = await _cache.GetStringAsync(key);
            return json is null ? default : JsonSerializer.Deserialize<T>(json);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var options = new DistributedCacheEntryOptions();
            if (expiry.HasValue)
                options.AbsoluteExpirationRelativeToNow = expiry;
            await _cache.SetStringAsync(key, JsonSerializer.Serialize(value), options);
        }

        public async Task RemoveAsync(string key) => await _cache.RemoveAsync(key);
    }
}
