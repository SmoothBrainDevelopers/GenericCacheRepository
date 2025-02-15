using GenericCacheRepository.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericCacheRepository.Interfaces
{
    public interface ICacheFactory
    {
        public CacheService CreateCacheService();
        public CompositeCacheService CreateCompositeCacheService();
    }
}
