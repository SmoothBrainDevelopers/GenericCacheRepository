using GenericCacheRepository.Helpers;
using GenericCacheRepository.Interfaces;
using GenericCacheRepository.Services;
using Microsoft.EntityFrameworkCore;
namespace GenericCacheRepository.Repository
{
    public class CacheRepository : ICacheRepository
    {
        private readonly ICacheService _cacheService;
        private readonly ICompositeCacheService _compositeCacheService;
        private readonly DbContext _dbContext;

        public CacheRepository(ICacheService cacheService, ICompositeCacheService compositeCacheService, DbContext dbContext)
        {
            _cacheService = cacheService;
            _compositeCacheService = compositeCacheService;
            _dbContext = dbContext;
        }

        public async Task<T?> FetchAsync<T>(params object[] keys) where T : class
        {
            string cacheKey = $"{typeof(T).Name}:{string.Join(":", keys)}";
            var cachedItem = await _cacheService.GetAsync<T>(cacheKey);

            if (cachedItem != null)
                return cachedItem;

            var dbSet = _dbContext.Set<T>();
            var entity = await dbSet.FindAsync(keys);

            if (entity != null)
                await _cacheService.SetAsync(cacheKey, entity, TimeSpan.FromMinutes(10));

            return entity;
        }

        public async Task<List<T>> FetchAsync<T>(int page, int pageCount, Query<T> query) where T : class
        {
            string compositeKey = query.GetCacheKey();
            var cachedIds = await _compositeCacheService.GetCachedIdsAsync(compositeKey);

            if (cachedIds != null && cachedIds.Count > 0)
            {
                var results = new List<T>();
                foreach (var id in cachedIds)
                {
                    var entity = await FetchAsync<T>(id);
                    if (entity != null) results.Add(entity);
                }
                return results;
            }

            var dbSet = _dbContext.Set<T>().AsQueryable();
            var filteredQuery = query.Apply(dbSet);
            var result = await filteredQuery.Skip((page - 1) * pageCount).Take(pageCount).ToListAsync();

            var ids = result.Select(x => (object)x.GetType().GetProperty("Id")?.GetValue(x)).Where(id => id != null).ToList();
            await _compositeCacheService.SetCachedIdsAsync(compositeKey, ids, TimeSpan.FromMinutes(10));

            return result;
        }

        public Task SaveAsync<T>(T entity) where T : class
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync<T>(params object[] keys) where T : class
        {
            throw new NotImplementedException();
        }
    }
}
