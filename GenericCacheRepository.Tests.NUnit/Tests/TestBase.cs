using Bogus;
using GenericCacheRepository.Interfaces;
using GenericCacheRepository.Repository;
using GenericCacheRepository.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SqlDbContextLib.DataLayer.Context;
using SqlDbContextLib.DataLayer.Domain;
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
        //shared instance with reference to open sqlite conneciton and builder options
        protected static SqliteDbContext<TestDbContext> _dbContext;
        private IMemoryCache _cache;
        protected CacheService _cacheService;
        protected ICompositeCacheService _compositeCacheService;
        protected CacheRepository<User> _repository;
        protected Mock<IDbContextProvider> _dbContextProviderMock;

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
            _dbContextProviderMock = new Mock<IDbContextProvider>();
            var loggerService = new Mock<ILoggerService>();

            //creates a new instance of the dbcontext with same builder optiosn on same open sqlite connection
            _dbContextProviderMock.Setup(x => x.GetDbContext()).Returns(() => _dbContext.CreateDbContext());

            _repository = new CacheRepository<User>(_cacheService, _compositeCacheService, _dbContextProviderMock.Object, loggerService.Object);
            SetupMock();
            RegisterTypes();
        }

        [TearDown]
        public void TearDown()
        {
            _cache.Dispose();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            //closes sqlite connection, garbage collects, and refreshes thread pool
            _dbContext.CloseConnection();
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

            _dbContext.RegisterKeyAssignment<User>((user, seeder, ctx) =>
            {
                user.Id = (int)seeder.IncrementKeys<User>().First();
            });
        }
    }
}
