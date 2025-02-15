using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericCacheRepository.Interfaces
{
    public interface ICompositeCacheService
    {
        public Task<List<object>> GetCachedIdsAsync(string compositeKey);
        public Task SetCachedIdsAsync(string compositeKey, List<object> ids, TimeSpan expiration);
        public Task RemoveCompositeKeyAsync(string compositeKey);
    }
}
