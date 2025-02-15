using GenericCacheRepository.Helpers;
using System.Collections.Generic;

namespace GenericCacheRepository.Interfaces
{
    public interface ICacheRepository
    {
        Task<T?> FetchAsync<T>(object key) where T : class;
        Task<List<T>> FetchAsync<T>(List<object> keys) where T : class;
        Task<List<T>> FetchAsync<T>(int page, int pageCount, Query<T> query) where T : class;
        Task SaveAsync<T>(T entity) where T : class;
        Task SaveBulkAsync<T>(List<T> entities) where T : class;
        Task DeleteAsync<T>(object key) where T : class;
        Task DeleteBulkAsync<T>(List<object> keys) where T : class;
    }

}
