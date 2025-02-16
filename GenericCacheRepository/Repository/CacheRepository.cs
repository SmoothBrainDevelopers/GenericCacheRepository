using GenericCacheRepository.Helpers;
using GenericCacheRepository.Interfaces;
using GenericCacheRepository.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

namespace GenericCacheRepository.Repository
{
    public class CacheRepository<T> : ICacheRepository<T> where T : class
    {
        private readonly ICacheService _cacheService;
        private readonly ICompositeCacheService _compositeCacheService;
        //private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILoggerService _logger;

        private readonly IDbContextProvider _dbContextProvider;

        public CacheRepository(
            ICacheService cacheService,
            ICompositeCacheService compositeCacheService,
            IDbContextProvider dbContextProvider,
            ILoggerService logger)
        {
            _cacheService = cacheService;
            _compositeCacheService = compositeCacheService;
            _dbContextProvider = dbContextProvider;
            _logger = logger;
        }


        public async Task<T?> FetchAsync(object key)
        {
            var tempKey = new List<object> { key };
            var result = await FetchAsync(tempKey);
            return result.FirstOrDefault();
        }

        public async Task<List<T>> FetchAsync(List<object> keys)
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

            using (var dbContext = _dbContextProvider.GetDbContext())
            {
                var dbSet = dbContext.Set<T>();

                var keyProperty = KeyResolver.GetKeyProperty<T>();
                if (keyProperty == null)
                    throw new InvalidOperationException($"No key property found for type {typeof(T).Name}");

                var keyType = keyProperty.PropertyType;
                var typedListType = typeof(List<>).MakeGenericType(keyType);
                var typedKeysList = Activator.CreateInstance(typedListType);
                var addMethod = typedListType.GetMethod("Add");

                foreach (var key in missingKeys)
                {
                    var convertedKey = Convert.ChangeType(key, keyType);
                    addMethod.Invoke(typedKeysList, new object[] { convertedKey });
                }

                var parameter = Expression.Parameter(typeof(T), "e");
                var propertyAccess = Expression.Call(
                    typeof(EF),
                    nameof(EF.Property),
                    new[] { keyType },
                    parameter,
                    Expression.Constant(keyProperty.Name)
                );

                var containsMethod = typedListType.GetMethod("Contains", new[] { keyType });
                var containsExpression = Expression.Call(Expression.Constant(typedKeysList), containsMethod, propertyAccess);
                var lambda = Expression.Lambda<Func<T, bool>>(containsExpression, parameter);

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
            }

            return cachedResults;
        }

        public async Task<List<T>> FetchAsync(int page, int pageCount, Query<T> query)
        {
            string compositeKey = query.GetCacheKey();
            var cachedIds = _compositeCacheService.GetCachedIds(compositeKey);

            if (cachedIds.Count > 0)
            {
                return await FetchAsync(cachedIds);
            }

            List<T> paginatedResults = null;
            using (var dbContext = _dbContextProvider.GetDbContext())
            {
                var dbSet = dbContext.Set<T>().AsQueryable();
                var filteredQuery = query.Apply(dbSet);

                paginatedResults = await filteredQuery
                    .Skip((page - 1) * pageCount)
                    .Take(pageCount)
                    .ToListAsync();

                var keyProperty = KeyResolver.GetKeyProperty<T>();
                if (keyProperty == null)
                    throw new InvalidOperationException($"No key property found for type {typeof(T).Name}");

                var ids = paginatedResults
                    .Select(entity => keyProperty.GetValue(entity))
                    .Where(id => id != null)
                    .ToList();

                _compositeCacheService.SetCachedIds(compositeKey, ids, TimeSpan.FromMinutes(10));
            }

            return paginatedResults ?? new List<T>();
        }

        public async Task SaveAsync(T entity)
        {
            using (var dbContext = _dbContextProvider.GetDbContext())
            {
                dbContext.Set<T>().Update(entity);
                await dbContext.SaveChangesAsync();

                var entityKey = KeyResolver.GetPrimaryKey(entity);
                if (entityKey != null)
                    await _cacheService.SetAsync($"{typeof(T).Name}:{entityKey}", entity, TimeSpan.FromMinutes(10));
            }
        }

        public async Task SaveBulkAsync(List<T> entities)
        {
            using (var dbContext = _dbContextProvider.GetDbContext())
            {
                dbContext.Set<T>().UpdateRange(entities);
                await dbContext.SaveChangesAsync();

                foreach (var entity in entities)
                {
                    var entityKey = KeyResolver.GetPrimaryKey(entity);
                    if (entityKey != null)
                        await _cacheService.SetAsync($"{typeof(T).Name}:{entityKey}", entity, TimeSpan.FromMinutes(10));
                }
            }
        }

        public async Task DeleteAsync(object key)
        {
            using (var dbContext = _dbContextProvider.GetDbContext())
            {
                var dbSet = dbContext.Set<T>();
                var entity = await dbSet.FindAsync(key);
                if (entity != null)
                {
                    dbSet.Remove(entity);
                    await dbContext.SaveChangesAsync();
                    await _cacheService.RemoveAsync($"{typeof(T).Name}:{key}");
                }
            }
        }

        public async Task DeleteBulkAsync(List<object> keys)
        {
            using (var dbContext = _dbContextProvider.GetDbContext())
            {
                var dbSet = dbContext.Set<T>();
                var entities = await FetchAsync(keys);
                if (entities.Any())
                {
                    dbSet.RemoveRange(entities);
                    await dbContext.SaveChangesAsync();

                    foreach (var key in keys)
                    {
                        await _cacheService.RemoveAsync($"{typeof(T).Name}:{key}");
                    }
                }

            }
        }
    }

}
