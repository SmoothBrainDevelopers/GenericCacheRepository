using Microsoft.EntityFrameworkCore;
using GenericCacheRepository.Interfaces;
using GenericCacheRepository.Helpers;
using GenericCacheRepository.Services;
using System.Linq.Expressions;
using System.Reflection;

namespace GenericCacheRepository.Repository
{
    public class CacheRepository<T> : ICacheRepository<T> where T : class
    {
        private readonly ICacheService _cacheService;
        private readonly IDbContextProvider _dbContextProvider;
        private readonly IEntityKeyService _entityKeyService;

        public CacheRepository(ICacheService cacheService, IDbContextProvider dbContextProvider, IEntityKeyService entityKeyService)
        {
            _cacheService = cacheService;
            _dbContextProvider = dbContextProvider;
            _entityKeyService = entityKeyService;
        }

        public async Task<T?> FetchAsync(params object[] keys)
        {
            using var dbContext = _dbContextProvider.GetDbContext();
            string cacheKey = string.Join(":", keys);
            var cachedItem = await _cacheService.GetAsync<T>(cacheKey);
            if (cachedItem != null) return cachedItem;

            var entityKeys = _entityKeyService.GetPrimaryKeys<T>(dbContext);
            var dbSet = dbContext.Set<T>();
            var query = dbSet.AsQueryable();

            foreach (var key in keys.Zip(entityKeys, Tuple.Create))
            {
                query = query.Where(e => EF.Property<object>(e, key.Item2.Name) == key.Item1);
            }

            var result = await query.FirstOrDefaultAsync();
            if (result != null)
                await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10));

            return result;
        }

        public async Task<List<T>> FetchListAsync(params object[] keys)
        {
            using var dbContext = _dbContextProvider.GetDbContext();
            var entityKeys = _entityKeyService.GetPrimaryKeys<T>(dbContext);
            var dbSet = dbContext.Set<T>();
            var query = dbSet.AsQueryable();

            foreach (var key in keys.Zip(entityKeys, Tuple.Create))
            {
                query = query.Where(e => EF.Property<object>(e, key.Item2.Name) == key.Item1);
            }

            return await query.ToListAsync();
        }

        public async Task<List<T>> FetchPageAsync(int page, int pageCount, string sortByPropertyName = null, bool asc = true)
        {
            using var dbContext = _dbContextProvider.GetDbContext();
            var dbSet = dbContext.Set<T>();
            var query = dbSet.AsQueryable();

            if (!string.IsNullOrEmpty(sortByPropertyName))
            {
                var property = typeof(T).GetProperty(sortByPropertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (property != null)
                {
                    var param = Expression.Parameter(typeof(T), "x");
                    var propertyAccess = Expression.Property(param, property);
                    var orderByExp = Expression.Lambda(propertyAccess, param);

                    string methodName = asc ? "OrderBy" : "OrderByDescending";
                    var orderByCall = Expression.Call(
                        typeof(Queryable),
                        methodName,
                        new Type[] { typeof(T), property.PropertyType },
                        query.Expression,
                        Expression.Quote(orderByExp));

                    query = query.Provider.CreateQuery<T>(orderByCall);
                }
            }

            return await query.Skip((page - 1) * pageCount).Take(pageCount).ToListAsync();
        }

        public async Task SaveAsync(T entity)
        {
            using var dbContext = _dbContextProvider.GetDbContext();
            dbContext.Set<T>().Update(entity);
            await dbContext.SaveChangesAsync();

            string cacheKey = _entityKeyService.GenerateCompositeKey(dbContext, entity);
            await _cacheService.SetAsync(cacheKey, entity, TimeSpan.FromMinutes(10));
        }

        public async Task SaveBulkAsync(List<T> entities)
        {
            using var dbContext = _dbContextProvider.GetDbContext();
            dbContext.Set<T>().UpdateRange(entities);
            await dbContext.SaveChangesAsync();

            foreach (var entity in entities)
            {
                string cacheKey = _entityKeyService.GenerateCompositeKey(dbContext, entity);
                await _cacheService.SetAsync(cacheKey, entity, TimeSpan.FromMinutes(10));
            }
        }

        public async Task DeleteAsync(params object[] keys)
        {
            using var dbContext = _dbContextProvider.GetDbContext();
            var entity = await FetchAsync(keys);
            if (entity != null)
            {
                dbContext.Set<T>().Remove(entity);
                await dbContext.SaveChangesAsync();

                string cacheKey = _entityKeyService.GenerateCompositeKey(dbContext, entity);
                await _cacheService.RemoveAsync(cacheKey);
            }
        }

        public async Task DeleteBulkAsync(List<object[]> keySets)
        {
            using var dbContext = _dbContextProvider.GetDbContext();
            foreach (var keys in keySets)
            {
                var entity = await FetchAsync(keys);
                if (entity != null)
                {
                    dbContext.Set<T>().Remove(entity);
                    await _cacheService.RemoveAsync(_entityKeyService.GenerateCompositeKey(dbContext, entity));
                }
            }
            await dbContext.SaveChangesAsync();
        }
    }
}
