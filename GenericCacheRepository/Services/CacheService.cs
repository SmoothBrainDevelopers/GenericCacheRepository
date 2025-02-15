using GenericCacheRepository.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericCacheRepository.Services
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _cache;

        public CacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            return _cache.TryGetValue(key, out T value) ? value : default;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            _cache.Set(key, value, expiration);
        }

        public async Task RemoveAsync(string key)
        {
            _cache.Remove(key);
        }
    }

}
