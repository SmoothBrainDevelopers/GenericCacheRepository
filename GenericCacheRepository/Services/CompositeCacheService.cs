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

        public List<object> GetCachedIds(string compositeKey)
        {
            _cache.TryGetValue(compositeKey, out List<object> cachedIds);
            return cachedIds ?? new List<object>();
        }

        public void SetCachedIds(string compositeKey, List<object> ids, TimeSpan expiration)
        {
            _cache.Set(compositeKey, ids, expiration);
        }

        public void RemoveCompositeKey(string compositeKey)
        {
            _cache.Remove(compositeKey);
        }
    }

}
