using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericCacheRepository.Interfaces
{
    public interface ICacheService
    {
        public Task<T?> GetAsync<T>(string key);
        public Task SetAsync<T>(string key, T value, TimeSpan expiration);
        public Task RemoveAsync(string key);
    }
}
