using Bogus;
using GenericCacheRepository.Interfaces;
using GenericCacheRepository.Repository;
using GenericCacheRepository.Services;
using GenericCacheRepository.Test.MS.Context;
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
        protected CacheRepository _repository;
        private IServiceCollection _services;

        [TestInitialize]
        public void Setup()
        {
            _dbContextMock = new SqliteDbContext<TestDbContext>();
            _cacheServiceMock = new Mock<ICacheService>();
            _compositeCacheServiceMock = new Mock<ICompositeCacheService>();
            var loggserService = new Mock<ILoggerService>();

            _services = new ServiceCollection();
            _services.AddDbContext<TestDbContext>();
            var serviceScopeFactory = _services.BuildServiceProvider().GetService<IServiceScopeFactory>();
            _repository = new CacheRepository(loggserService.Object, _cacheServiceMock.Object, _compositeCacheServiceMock.Object, serviceScopeFactory, _dbContextMock.Context);
            SetupMock();
            RegisterTypes();
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
