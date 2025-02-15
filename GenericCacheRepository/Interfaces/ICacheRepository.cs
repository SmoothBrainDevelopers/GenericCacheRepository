using GenericCacheRepository.Helpers;
using System.Collections.Generic;

namespace GenericCacheRepository.Interfaces
{
    public interface ICacheRepository
    {
        Task<T?> FetchAsync<T>(params object[] keys) where T : class;
        Task<List<T>> FetchAsync<T>(int page, int pageCount, Query<T> query) where T : class;
        Task SaveAsync<T>(T entity) where T : class;
        Task DeleteAsync<T>(params object[] keys) where T : class;
        Task<List<T>> FetchAsync<T>(List<object> keys) where T : class;
    }


}
