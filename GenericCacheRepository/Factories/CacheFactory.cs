using GenericCacheRepository.Interfaces;
using GenericCacheRepository.Services;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericCacheRepository.Factories
{
    public class CacheFactory : ICacheFactory
    {
        private readonly IMemoryCache _memoryCache;

        public CacheFactory(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public CacheService CreateCacheService() => new CacheService(_memoryCache);
        public CompositeCacheService CreateCompositeCacheService() => new CompositeCacheService(_memoryCache);
    }

}
