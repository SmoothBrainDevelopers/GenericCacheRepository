using GenericCacheRepository.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace GenericCacheRepository.Interfaces
{
    public interface ICacheRepository<T> where T : class
    {
        Task<T?> FetchAsync(params object[] keys);
        Task<List<T>> FetchListAsync(params object[] keys);
        Task SaveAsync(T entity);
        Task SaveBulkAsync(List<T> entities);
        Task DeleteAsync(params object[] keys);
        Task DeleteBulkAsync(List<object[]> keySets);
    }

}
