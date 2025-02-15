using GenericCacheRepository.Helpers;
using GenericCacheRepository.Interfaces;
using GenericCacheRepository.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GenericCacheRepository.Repository
{
    public class CacheRepository : ICacheRepository
    {
        private readonly ILoggerService _logger;
        private readonly ICacheService _cacheService;
        private readonly ICompositeCacheService _compositeCacheService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly DbContext _dbContext;

        public CacheRepository(ILoggerService logger, ICacheService cacheService, ICompositeCacheService compositeCacheService, IServiceScopeFactory serviceScopeFactory, DbContext dbContext)
        {
            _logger = logger;
            _cacheService = cacheService;
            _compositeCacheService = compositeCacheService;
            _dbContext = dbContext;
        }

        private DbContext CreateDbContext()
        {
            using var scope = _serviceScopeFactory.CreateScope();
            return scope.ServiceProvider.GetRequiredService<DbContext>();
        }

        public async Task<List<T>> FetchAsync<T>(int page, int pageCount, Query<T> query) where T : class
        {
            string compositeKey = query.GetCacheKey();
            var cachedIds = await _compositeCacheService.GetCachedIdsAsync(compositeKey);

            if (cachedIds.Count > 0)
            {
                var results = await FetchAsync<T>(cachedIds);
                return results;
            }

            using var dbContext = CreateDbContext();
            var dbSet = dbContext.Set<T>().AsQueryable();
            var filteredQuery = query.Apply(dbSet);
            var result = await filteredQuery.Skip((page - 1) * pageCount).Take(pageCount).ToListAsync();

            var ids = result.Select(x => (object)x.GetType().GetProperty("Id")?.GetValue(x)).Where(id => id != null).ToList();
            await _compositeCacheService.SetCachedIdsAsync(compositeKey, ids, TimeSpan.FromMinutes(10));

            return result;
        }

        public async Task<List<T>> FetchAsync<T>(List<object> keys) where T : class
        {
            string cacheKey = $"{typeof(T).Name}:{string.Join(":", keys)}";
            var cachedItems = new List<T>();

            foreach (var key in keys)
            {
                var cachedItem = await _cacheService.GetAsync<T>($"{typeof(T).Name}:{key}");
                if (cachedItem != null)
                {
                    cachedItems.Add(cachedItem);
                }
            }

            if (cachedItems.Count == keys.Count) return cachedItems;

            using var dbContext = CreateDbContext();
            var dbSet = dbContext.Set<T>();
            var missingKeys = keys.Except(cachedItems.Select(x => x.GetType().GetProperty("Id")?.GetValue(x)));

            var retrievedEntities = await dbSet.Where(e => e.GetType().GetProperty("Id") == null ? false : missingKeys.Contains(e.GetType().GetProperty("Id")!.GetValue(e))).ToListAsync();

            foreach (var entity in retrievedEntities)
            {
                var entityKey = entity.GetType().GetProperty("Id")?.GetValue(entity);
                if (entityKey != null)
                    await _cacheService.SetAsync($"{typeof(T).Name}:{entityKey}", entity, TimeSpan.FromMinutes(10));
            }

            cachedItems.AddRange(retrievedEntities);
            return cachedItems;
        }

        public async Task RefreshCompositeCacheAsync<T>(string compositeKey, List<object> ids)
        {
            var accessCount = (await _cacheService.GetAsync<int>(compositeKey + "_accessCount")) + 1;
            TimeSpan expiration = accessCount > 5 ? TimeSpan.FromMinutes(30) : TimeSpan.FromMinutes(10);
            await _compositeCacheService.SetCachedIdsAsync(compositeKey, ids, expiration);
        }

        public async Task<List<T>> FetchMultipleAsync<T>(List<object> keys) where T : class
        {
            var fetchTasks = keys.Select(key => FetchAsync<T>(key)).ToList();
            var results = await Task.WhenAll(fetchTasks);
            return results.Where(r => r != null).ToList();
        }

        public Task DeleteAsync<T>(params object[] keys) where T : class
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync<T>(object key) where T : class
        {
            throw new NotImplementedException();
        }

        public Task DeleteBulkAsync<T>(List<object> keys) where T : class
        {
            throw new NotImplementedException();
        }

        public Task<T?> FetchAsync<T>(object key) where T : class
        {
            throw new NotImplementedException();
        }

        public Task SaveAsync<T>(T entity) where T : class
        {
            throw new NotImplementedException();
        }

        public Task SaveBulkAsync<T>(List<T> entities) where T : class
        {
            throw new NotImplementedException();
        }
    }
}
