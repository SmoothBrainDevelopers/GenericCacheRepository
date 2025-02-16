using GenericCacheRepository.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace GenericCacheRepository.Interfaces
{
    public interface ICacheRepository<T> where T : class
    {
        Task<T?> FetchAsync(object key);
        Task<List<T>> FetchAsync(List<object> keys);
        Task<List<T>> FetchAsync(int page, int pageCount, Query<T> query);
        Task SaveAsync(T entity);
        Task SaveBulkAsync(List<T> entities);
        Task DeleteAsync(object key);
        Task DeleteBulkAsync(List<object> keys);
    }

}
