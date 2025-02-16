using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericCacheRepository.Interfaces
{
    public interface ICompositeCacheService
    {
        public List<object> GetCachedIds(string compositeKey);
        public void SetCachedIds(string compositeKey, List<object> ids, TimeSpan expiration);
        public void RemoveCompositeKey(string compositeKey);
    }
}
