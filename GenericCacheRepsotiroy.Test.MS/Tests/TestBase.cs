using Bogus;
using GenericCacheRepository.Interfaces;
using GenericCacheRepository.Repository;
using GenericCacheRepository.Services;
using GenericCacheRepository.Test.MS.Context;
using Microsoft.Data.Sqlite;
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
        protected SqliteDbContext<TestDbContext> _dbContextMock;
        protected Mock<ICacheService> _cacheServiceMock;
        protected Mock<ICompositeCacheService> _compositeCacheServiceMock;
        protected CacheRepository<TestDbContext> _repository;
        private Mock<IServiceScopeFactory> _serviceScopeFactory;

        [TestInitialize]
        public void Setup()
        {
            _cacheServiceMock = new Mock<ICacheService>();
            _compositeCacheServiceMock = new Mock<ICompositeCacheService>();
            var loggserService = new Mock<ILoggerService>();

            SetupDbContext();
            
            _repository = new CacheRepository<TestDbContext>(_cacheServiceMock.Object, _compositeCacheServiceMock.Object, _serviceScopeFactory.Object, loggserService.Object);
            _repository.IsTest = true;
            SetupMock();
            RegisterTypes();
        }

        private void SetupDbContext()
        {
            _dbContextMock = new SqliteDbContext<TestDbContext>("Test");
            _dbContextMock.Context.Database.EnsureDeleted();
            _dbContextMock.Context.Database.EnsureCreated();
            //var services = new ServiceCollection();
            //services.AddDbContext<TestDbContext>(options => options.UseInMemoryDatabase("Test:memory:"));
            //var serviceScopeFactory = services.BuildServiceProvider().GetService<IServiceScopeFactory>();
            //_serviceScopeFactory = serviceScopeFactory;
            _serviceScopeFactory = new Mock<IServiceScopeFactory>();
            var scope = new Mock<IServiceScope>();
            var serviceProvider = new Mock<IServiceProvider>();
            _serviceScopeFactory.Setup(s => s.CreateScope()).Returns(() => scope.Object);
            scope.Setup(s => s.ServiceProvider).Returns(() => serviceProvider.Object);
            serviceProvider.Setup(s => s.GetService(typeof(TestDbContext))).Returns(() => _dbContextMock.Context);
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
