using GenericCacheRepository.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace GenericCacheRepository.Services
{
    public class CompositeCacheService : ICompositeCacheService
    {
        private readonly IMemoryCache _cache;

        public CompositeCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<List<object>> GetCachedIdsAsync(string compositeKey)
        {
            return _cache.TryGetValue(compositeKey, out List<object> cachedIds) ? cachedIds : new List<object>();
        }

        public async Task SetCachedIdsAsync(string compositeKey, List<object> ids, TimeSpan expiration)
        {
            _cache.Set(compositeKey, ids, expiration);
        }

        public async Task RemoveCompositeKeyAsync(string compositeKey)
        {
            _cache.Remove(compositeKey);
        }
    }

}
