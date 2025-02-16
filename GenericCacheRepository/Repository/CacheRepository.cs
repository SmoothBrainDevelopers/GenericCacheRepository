using GenericCacheRepository.Helpers;
using GenericCacheRepository.Interfaces;
using GenericCacheRepository.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

namespace GenericCacheRepository.Repository
{
    public class CacheRepository<T> : ICacheRepository<T> where T : DbContext, new()
    {
        private readonly ICacheService _cacheService;
        private readonly ICompositeCacheService _compositeCacheService;
        //private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILoggerService _logger;
        public bool IsTest { get; set; }
        private readonly T _dbContext;

        public CacheRepository(ICacheService cacheService, ICompositeCacheService compositeCacheService, T dbContext, ILoggerService logger)
        {
            _cacheService = cacheService;
            _compositeCacheService = compositeCacheService;
            //_serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _dbContext = dbContext;
        }

        private async Task UsingDbContextAsync(Func<T, Task> task)
        {
            await task(_dbContext);
            //using (var scope = _serviceScopeFactory.CreateScope())
            //{
            //    var dbContext = scope.ServiceProvider.GetRequiredService<T>();
            //    await task(dbContext);
            //}
        }

        public async Task<T?> FetchAsync<T>(object key) where T : class
        {
            var tempKey = new List<object> { key };
            var results = await FetchAsync<T>(tempKey);
            return results.FirstOrDefault();
        }

        public async Task<List<T>> FetchAsync<T>(List<object> keys) where T : class
        {
            var cachedResults = new List<T>();
            var missingKeys = new List<object>();

            foreach (var key in keys)
            {
                var cachedItem = await _cacheService.GetAsync<T>($"{typeof(T).Name}:{key}");
                if (cachedItem != null)
                {
                    cachedResults.Add(cachedItem);
                }
                else
                {
                    missingKeys.Add(key);
                }
            }

            if (!missingKeys.Any()) return cachedResults;

            await UsingDbContextAsync(async dbContext =>
            {
                var dbSet = dbContext.Set<T>();

                var lambda = KeyResolver.GetFetchByKeysLambda<T>(missingKeys);

                // Execute query
                var retrievedEntities = await dbSet.Where(lambda).ToListAsync();

                foreach (var entity in retrievedEntities)
                {
                    var entityKey = KeyResolver.GetPrimaryKey(entity);
                    if (entityKey != null)
                    {
                        await _cacheService.SetAsync($"{typeof(T).Name}:{entityKey}", entity, TimeSpan.FromMinutes(10));
                        cachedResults.Add(entity);
                    }
                }
            });
            return cachedResults ?? new List<T>();
        }

        public async Task<List<T>> FetchAsync<T>(int page, int pageCount, Query<T> query) where T : class
        {
            string compositeKey = query.GetCacheKey();
            var cachedIds = _compositeCacheService.GetCachedIds(compositeKey);

            if (cachedIds.Count > 0)
            {
                var cacheResults = await FetchAsync<T>(cachedIds);
                if(cacheResults != null && cacheResults.Any())
                {
                    return cacheResults;
                }
            }

            List<T> paginatedResults = null;
            await UsingDbContextAsync(async dbContext =>
            {
                var dbSet = dbContext.Set<T>().AsQueryable();

                // Apply query filters
                var filteredQuery = query.Apply(dbSet);

                // Paginate
                paginatedResults = await filteredQuery
                    .Skip((page - 1) * pageCount)
                    .Take(pageCount)
                    .ToListAsync();

                // Get the primary key property
                var keyProperty = KeyResolver.GetKeyProperty<T>();
                if (keyProperty == null)
                {
                    throw new InvalidOperationException($"No key property found for type {typeof(T).Name}");
                }

                // Convert paginated results into a strongly-typed key list
                var keyType = keyProperty.PropertyType;
                var typedListType = typeof(List<>).MakeGenericType(keyType);
                var typedKeysList = Activator.CreateInstance(typedListType);
                var addMethod = typedListType.GetMethod("Add");

                var ids = new List<object>();
                foreach (var entity in paginatedResults)
                {
                    var entityKey = keyProperty.GetValue(entity);
                    if (entityKey != null)
                    {
                        ids.Add(entityKey);
                        addMethod.Invoke(typedKeysList, new object[] { Convert.ChangeType(entityKey, keyType) });
                    }
                }

                // Cache the composite key mapping
                _compositeCacheService.SetCachedIds(compositeKey, ids, TimeSpan.FromMinutes(10));
            });
            return paginatedResults ?? new List<T>();
        }


        public async Task SaveAsync<T>(T entity) where T : class
        {
            await UsingDbContextAsync(async dbContext =>
            {
                dbContext.Set<T>().Update(entity);
                await dbContext.SaveChangesAsync();

                var entityKey = KeyResolver.GetPrimaryKey(entity);
                if (entityKey != null)
                    await _cacheService.SetAsync($"{typeof(T).Name}:{entityKey}", entity, TimeSpan.FromMinutes(10));
            });
        }


        public async Task SaveBulkAsync<T>(List<T> entities) where T : class
        {
            await UsingDbContextAsync(async dbContext =>
            {
                dbContext.Set<T>().UpdateRange(entities);
                await dbContext.SaveChangesAsync();

                foreach (var entity in entities)
                {
                    var entityKey = entity.GetType().GetProperty("Id")?.GetValue(entity);
                    if (entityKey != null)
                        await _cacheService.SetAsync($"{typeof(T).Name}:{entityKey}", entity, TimeSpan.FromMinutes(10));
                }
            });
        }

        public async Task DeleteAsync<T>(object key) where T : class
        {
            await UsingDbContextAsync(async dbContext =>
            {
                var dbSet = dbContext.Set<T>();

                var entity = await dbSet.FindAsync(key);
                if (entity != null)
                {
                    dbSet.Remove(entity);
                    await dbContext.SaveChangesAsync();
                    await _cacheService.RemoveAsync($"{typeof(T).Name}:{key}");
                }
            });
        }

        public async Task DeleteBulkAsync<T>(List<object> keys) where T : class
        {
            await UsingDbContextAsync(async dbContext =>
            {
                var dbSet = dbContext.Set<T>();

                var entities = await dbSet.Where(e => keys.Contains(e.GetType().GetProperty("Id") == null ? false : e.GetType().GetProperty("Id")!.GetValue(e))).ToListAsync();
                if (entities.Any())
                {
                    dbSet.RemoveRange(entities);
                    await dbContext.SaveChangesAsync();

                    foreach (var key in keys)
                    {
                        await _cacheService.RemoveAsync($"{typeof(T).Name}:{key}");
                    }
                }
            });
        }
    }
}
