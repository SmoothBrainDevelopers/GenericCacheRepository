using GenericCacheRepository.Helpers;
using GenericCacheRepository.Interfaces;
using GenericCacheRepository.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericCacheRepository.Repository
{
    public class CacheRepository : ICacheRepository
    {
        private readonly ICacheService _cacheService;
        private readonly DbContext _dbContext;

        public CacheRepository(ICacheService cacheService, DbContext dbContext)
        {
            _cacheService = cacheService;
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
            var dbSet = _dbContext.Set<T>().AsQueryable();
            var filteredQuery = query.Apply(dbSet);
            var result = await filteredQuery.Skip((page - 1) * pageCount).Take(pageCount).ToListAsync();
            return result;
        }

        public async Task SaveAsync<T>(T entity) where T : class
        {
            var dbSet = _dbContext.Set<T>();
            dbSet.Update(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync<T>(params object[] keys) where T : class
        {
            var entity = await FetchAsync<T>(keys);
            if (entity != null)
            {
                var dbSet = _dbContext.Set<T>();
                dbSet.Remove(entity);
                await _dbContext.SaveChangesAsync();
            }
        }
    }

}
