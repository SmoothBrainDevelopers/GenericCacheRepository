using Bogus;
using GenericCacheRepository.Interfaces;
using GenericCacheRepository.Repository;
using GenericCacheRepository.Services;
using GenericCacheRepository.Test.MS.Context;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SqliteDbContext.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericCacheRepository.Test.MS.Tests
{
    public class TestBase
    {
        private static readonly object _lock = new object();
        private static bool _dbInitialized = false;

        private IMemoryCache _cache;
        protected SqliteDbContext<TestDbContext> _dbContext;
        protected CacheService _cacheService;
        protected ICompositeCacheService _compositeCacheService;
        protected CacheRepository<TestDbContext> _repository;
        private Mock<IServiceScopeFactory> _serviceScopeFactory;

        [TestInitialize]
        public void Setup()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
            _cacheService = new CacheService(_cache);
            _compositeCacheService = new CompositeCacheService(_cache);
            var loggserService = new Mock<ILoggerService>();

            SetupDbContext();
            
            _repository = new CacheRepository<TestDbContext>(_cacheService, _compositeCacheService, _serviceScopeFactory.Object, loggserService.Object);
            _repository.IsTest = true;
            SetupMock();
            RegisterTypes();
        }

        private void SetupDbContext()
        {
            lock (_lock)
            {
                if (!_dbInitialized)
                {
                    _dbContext = new SqliteDbContext<TestDbContext>("Test");
                    _dbContext.Context.Database.EnsureDeleted();
                    _dbContext.Context.Database.EnsureCreated();
                    _dbInitialized = true;

                    // Ensure each test gets a new scoped DbContext
                    _serviceScopeFactory = new Mock<IServiceScopeFactory>();
                    var scope = new Mock<IServiceScope>();
                    var serviceProvider = new Mock<IServiceProvider>();

                    _serviceScopeFactory.Setup(s => s.CreateScope()).Returns(() => scope.Object);
                    scope.Setup(s => s.ServiceProvider).Returns(() => serviceProvider.Object);
                    serviceProvider.Setup(s => s.GetService(typeof(TestDbContext))).Returns(_dbContext.Context);
                }
                else
                {
                    _dbContext = new SqliteDbContext<TestDbContext>("Test");
                }
            }
        }

        private void SetupMock()
        {
        }

        private static HashSet<Type> _registeredTypes = new HashSet<Type>();
        private static void RegisterTypes()
        {
            RegisterType<User>(() =>
            {
                SqliteDbContext<TestDbContext>.RegisterKeyAssignment<User>((user, seeder) =>
                {
                    user.Id = (int)seeder.IncrementKeys<User>().First();
                });
            });
            
        }

        private static void RegisterType<T>(Action action)
        {
            if (_registeredTypes.Contains(typeof(T)))
                return;
            _registeredTypes.Add(typeof(T));
            action();
        }
    }
}
