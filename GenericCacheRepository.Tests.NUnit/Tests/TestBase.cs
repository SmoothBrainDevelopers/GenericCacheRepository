using GenericCacheRepository.Interfaces;
using GenericCacheRepository.Repository;
using GenericCacheRepository.Services;
using GenericCacheRepository.Tests.NUnit.Context;
using GenericCacheRepository.Tests.NUnit.Domain;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SqliteDbContext.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericCacheRepository.Tests.NUnit.Tests
{
    public class TestBase
    {
        protected static SqliteDbContext<TestDbContext> _dbContext;
        private IMemoryCache _cache;
        protected CacheService _cacheService;
        protected ICompositeCacheService _compositeCacheService;
        protected CacheRepository<TestDbContext> _repository;
        private Mock<IServiceScopeFactory> _serviceScopeFactory;

        [OneTimeSetUp]
        public static void OneTimeSetUp()
        {
            SetupDbContext();
        }

        [SetUp]
        public void Setup()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
            _cacheService = new CacheService(_cache);
            _compositeCacheService = new CompositeCacheService(_cache);
            var loggserService = new Mock<ILoggerService>();

            _repository = new CacheRepository<TestDbContext>(_cacheService, _compositeCacheService, _dbContext.Context, /*_serviceScopeFactory.Object, */loggserService.Object);
            _repository.IsTest = true;
            SetupMock();
            RegisterTypes();
        }

        [TearDown]
        public void TearDown()
        {
            _cache.Dispose();
        }


        private static void SetupDbContext()
        {
            _dbContext = new SqliteDbContext<TestDbContext>("Test");
            _dbContext.Context.Database.EnsureDeleted();
            _dbContext.Context.Database.EnsureCreated();
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
