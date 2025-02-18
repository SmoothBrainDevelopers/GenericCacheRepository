using Bogus;
using GenericCacheRepository.Interfaces;
using GenericCacheRepository.Repository;
using GenericCacheRepository.Services;
using GenericCacheRepository.Tests.NUnit.Context;
using GenericCacheRepository.Tests.NUnit.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SqliteDbContext.Context;
using SQLitePCL;
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
        protected ICacheRepository<User> _userCacheRepository { get; set; }
        protected ICacheRepository<Purchase> _purchaseCacheRepository { get; set; }
        protected Mock<IDbContextProvider> _dbContextProviderMock;
        private IEntityKeyService _entityKeyService;

        [OneTimeSetUp]
        public static void OneTimeSetUp()
        {
            SetupDbContext();
            RegisterTypes();
            SeedData();
        }

        [SetUp]
        public void Setup()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
            _cacheService = new CacheService(_cache);
            _entityKeyService = new EntityKeyService();
            _compositeCacheService = new CompositeCacheService(_cache);
            _dbContextProviderMock = new Mock<IDbContextProvider>();
            //creates a new instance of the dbcontext with same builder optiosn on same open sqlite connection
            _dbContextProviderMock.Setup(x => x.GetDbContext()).Returns(() => _dbContext.CopyDbContext());
            SetupCacheRepositories();
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

        private void SetupCacheRepositories()
        {
            var loggerService = new Mock<ILoggerService>();
            _userCacheRepository = new CacheRepository<User>(_cacheService, _dbContextProviderMock.Object, _entityKeyService);
            _purchaseCacheRepository = new CacheRepository<Purchase>(_cacheService, _dbContextProviderMock.Object, _entityKeyService);
        }

        private static HashSet<Type> _registeredTypes = new HashSet<Type>();
        private static void RegisterTypes()
        {

            //_dbContext.RegisterKeyAssignment<User>((user, seeder, ctx) =>
            //{
            //    user.Id = (int)seeder.IncrementKeys<User>().First();
            //});

            //_dbContext.RegisterKeyAssignment<Customer>((customer, seeder, ctx) =>
            //{
            //    customer.CustomerId = (int)seeder.IncrementKeys<Customer>().First();
            //});

            //_dbContext.RegisterKeyAssignment<Store>((store, seeder, ctx) =>
            //{
            //    store.StoreId = (int)seeder.IncrementKeys<Store>().First();
            //    store.RegionId = (int)seeder.GetRandomKeys<Region>().First();
            //});

            //_dbContext.RegisterKeyAssignment<Product>((product, seeder, ctx) =>
            //{
            //    product.ProductId = (int)seeder.IncrementKeys<Product>().First();
            //});

            //_dbContext.RegisterKeyAssignment<Purchase>((purchase, seeder, ctx) =>
            //{
            //    purchase.CustomerId = (int)seeder.GetRandomKeys<Customer>().First();
            //    purchase.StoreId = (int)seeder.GetRandomKeys<Store>().First();
            //    purchase.ProductId = (int)seeder.GetRandomKeys<Product>().First();
            //});

            //_dbContext.RegisterKeyAssignment<Region>((region, seeder, ctx) =>
            //{
            //    region.RegionId = (int)seeder.IncrementKeys<Region>().First();
            //});

            //_dbContext.RegisterKeyAssignment<Sale>((sale, seeder, ctx) =>
            //{
            //    sale.SaleId = (int)seeder.IncrementKeys<Sale>().First();
            //    sale.StoreId = (int)seeder.GetRandomKeys<Store>().First();
            //});
        }

        private static void SeedData()
        {
            _dbContext.GenerateEntities<Customer>(10);
            _dbContext.GenerateEntities<Region>(2);
            _dbContext.GenerateEntities<Product>(10);
            _dbContext.GenerateEntities<Store>(2);
            _dbContext.GenerateEntities<Sale>(3);
            _dbContext.GenerateEntities<Purchase>(50);
        }
    }
}
